using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraFiltre.Lib
{
    public class GestionFiltres
    {
        public static Expression<Func<TTypeCible, bool>> GetExpression<TTypeCible>(IList<Filtre> pFiltres)
        {
            if (pFiltres == null || pFiltres.Count == 0)
                return (_) => true;

            ParameterExpression parameterExpression = Expression.Parameter(typeof(TTypeCible), "racine");
            Expression lExpression = FiltresToExpression(parameterExpression, pFiltres);
            if (lExpression == null)
                return (_) => true;

            return Expression.Lambda<Func<TTypeCible, bool>>(lExpression, parameterExpression);
        }

        // Permet de convertir un groupe de filtre en une expression
        private static Expression FiltresToExpression(Expression pParent, IList<Filtre> pFiltres, int pProfondeur = 0)
        {
            Expression lExpression = null;

            foreach (var lFiltre in pFiltres)
            {
                Expression lFiltreExpression = null;
                Expression lPropCible;

                // Gérer une propriété null (dans le cas d'un groupe logique) et prendre la propriété du parent
                if (lFiltre.Propriete == null)
                    lPropCible = pParent;
                else
                    lPropCible = Expression.Property(pParent, lFiltre.Propriete);

                // Gestion des listes
                if (lFiltre.Expression is FiltreGroupe lGroupeListe && Utils.EstListe(lPropCible.Type))
                    lFiltreExpression = ListeToExpression(lPropCible, lGroupeListe, pProfondeur);
                // Gestion groupes
                else if (lFiltre.Expression is FiltreGroupe lGroupeObjet)
                    lFiltreExpression = ObjetToExpression(lPropCible, lGroupeObjet, pProfondeur);
                else if (lFiltre.Expression is FiltreValeur lFiltreValeur)
                    lFiltreExpression = ValeurToExpression(lPropCible, lFiltreValeur);

                // Conjonction
                if (lExpression == null)
                    lExpression = lFiltreExpression;
                else
                {
                    switch (lFiltre.Conjonction)
                    {
                        case EnumConjonctionFiltre.And:
                            lExpression = Expression.AndAlso(lExpression, lFiltreExpression);
                            break;
                        case EnumConjonctionFiltre.Or:
                            lExpression = Expression.OrElse(lExpression, lFiltreExpression);
                            break;
                    }
                }
            }
            return lExpression;
        }

        private static Expression ObjetToExpression(Expression pPropCible, FiltreGroupe pGroupeObjet, int pProfondeur)
        { return FiltresToExpression(pPropCible, pGroupeObjet.Enfants, pProfondeur + 1); }

        // Oui c'est de l'anglais, parce que IA
        private static Expression ListeToExpression(Expression pPropCible, FiltreGroupe pGroupeListe, int pProfondeur)
        {
            // Create a new expression on pPropCible with a call to the method Where of the class Enumerable and then call the method Any
            var lMethodWhere = typeof(Enumerable).GetMethods()
                .First(m => m.Name == nameof(Enumerable.Where) && m.GetParameters().Length == 2);
            var lMethodAny = typeof(Enumerable).GetMethods()
                .First(m => m.Name == nameof(Enumerable.Any) && m.GetParameters().Length == 1);

            // Get pPropCible property name
            string lNomParam;
            if (pPropCible is ParameterExpression lParamExpression)
                lNomParam = lParamExpression.Name;
            else if (pPropCible is MemberExpression lMemberExpression)
                lNomParam = lMemberExpression.Member.Name;
            else
                throw new NotImplementedException($"Le type [{pPropCible.GetType()}] n'est pas géré.");

            // Create the lambda expression for the Where method
            var lParam = Expression.Parameter(pPropCible.Type.GetGenericArguments().First(), $"{lNomParam}_{pProfondeur}");
            var lLambda = Expression.Lambda(FiltresToExpression(lParam, pGroupeListe.Enfants, pProfondeur + 1), lParam);

            // Allow null value for pPropCible (final result will be a bool?) 
            if (pPropCible.Type.GetGenericArguments().First().IsValueType)
                pPropCible = Expression.Coalesce(
                    pPropCible,
                    Expression.Constant(Activator.CreateInstance(pPropCible.Type.GetGenericArguments().First())));

            // Call the Where method
            var lWhereCall = Expression.Call(
                lMethodWhere.MakeGenericMethod(pPropCible.Type.GetGenericArguments().First()),
                pPropCible,
                lLambda);

            // Call the Any method
            var lAnyCall = Expression.Call(
                lMethodAny.MakeGenericMethod(pPropCible.Type.GetGenericArguments().First()),
                lWhereCall);

            return lAnyCall;
        }

        private static Expression ValeurToExpression(Expression pPropCible, FiltreValeur pFiltreValeur)
        {
            var lConstante = Expression.Constant(Convert(pFiltreValeur.Valeur ?? "", pPropCible.Type));

            Expression lExpression = null;
            switch (pFiltreValeur.Operateur)
            {
                case EnumComparaisonFiltre.EqualsTo:
                case EnumComparaisonFiltre.NotEqualsTo:
                    lExpression = Expression.Equal(pPropCible, lConstante);
                    break;
                case EnumComparaisonFiltre.GreaterThan:
                    //case EnumComparaisonFiltre.NotGreaterThan:
                    lExpression = Expression.GreaterThan(pPropCible, lConstante);
                    break;
                case EnumComparaisonFiltre.GreaterThanOrEqual:
                    //case EnumComparaisonFiltre.NotGreaterThanOrEqual:
                    lExpression = Expression.GreaterThanOrEqual(pPropCible, lConstante);
                    break;
                case EnumComparaisonFiltre.LesserThan:
                    //case EnumComparaisonFiltre.NotLesserThan:
                    lExpression = Expression.LessThan(pPropCible, lConstante);
                    break;
                case EnumComparaisonFiltre.LesserThanOrEqual:
                    //case EnumComparaisonFiltre.NotLesserThanOrEqual:
                    lExpression = Expression.LessThanOrEqual(pPropCible, lConstante);
                    break;
                // Gestion des méthodes spécifiques aux String
                case EnumComparaisonFiltre.Contains:
                case EnumComparaisonFiltre.NotContains:
                    {
                        MethodInfo lMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
                        lExpression = Expression.Call(pPropCible, lMethod, lConstante);
                        break;
                    }
                case EnumComparaisonFiltre.StartsWith:
                case EnumComparaisonFiltre.NotStartsWith:
                    {
                        MethodInfo lMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
                        lExpression = Expression.Call(pPropCible, lMethod, lConstante);
                        break;
                    }
                case EnumComparaisonFiltre.EndsWith:
                case EnumComparaisonFiltre.NotEndsWith:
                    {
                        MethodInfo lMethod = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
                        lExpression = Expression.Call(pPropCible, lMethod, lConstante);
                        break;
                    }
                case EnumComparaisonFiltre.Between:
                case EnumComparaisonFiltre.NotBetween:
                default:
                    throw new NotImplementedException(
                        $"Le type de comparaison [{pFiltreValeur.Operateur}] n'est pas géré.");
            }

            // Gérer la négation
            return (pFiltreValeur.Operateur.ToString().StartsWith("Not")) ? Expression.Not(lExpression) : lExpression;
        }

        private static object Convert(object pValeur, Type pReturnType)
        {
            try
            {
                if (pValeur == null)
                    return null;

                if (pValeur.GetType().IsEnum && !pReturnType.IsEnum)
                    pValeur = (int)pValeur;

                if (pValeur.GetType() == pReturnType)
                    return pValeur;

                var lConverter = System.ComponentModel.TypeDescriptor.GetConverter(pReturnType);
                return lConverter.ConvertFrom(null, System.Globalization.CultureInfo.CurrentCulture, pValeur.ToString());
            }
            catch (Exception lEx)
            {
                throw new Exception($"La valeur [{pValeur}] ne peut pas être converti en type [{pReturnType}]", lEx);
            }
        }
    }
}
