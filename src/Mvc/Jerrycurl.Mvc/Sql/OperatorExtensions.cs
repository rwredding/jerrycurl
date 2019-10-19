using System;
using Jerrycurl.Mvc.Projections;

namespace Jerrycurl.Mvc.Sql
{
    public static class OperatorExtensions
    {
        /// <summary>
        /// Appends the assignment operator to the current projection buffer and changes the projection separator value to <c>"AND"</c>.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection IsEq(this IProjection projection)
        {
            ProjectionOptions newOptions = new ProjectionOptions(projection.Options)
            {
                Separator = Environment.NewLine + "AND" + Environment.NewLine,
            };

            return projection.Map(a => a.Eq()).With(options: newOptions);
        }

        /// <summary>
        /// Appends the equality operator to the current projection buffer and changes the projection separator value to <c>,</c>.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection Eq(this IProjection projection)
        {
            ProjectionOptions newOptions = new ProjectionOptions(projection.Options)
            {
                Separator = "," + Environment.NewLine,
            };

            return projection.Map(a => a.Eq()).With(options: newOptions);
        }


        /// <summary>
        /// Appends the <c>IS NULL</c> operator to the current attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute IsNull(this IProjectionAttribute attribute) => attribute.Append(" IS NULL");

        /// <summary>
        /// Appends the <c>IS NOT NULL</c> operator to the current attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute IsNotNull(this IProjectionAttribute attribute) => attribute.Append(" IS NOT NULL");

        /// <summary>
        /// Appends the <c> AS </c> keyword to the current projection buffer.
        /// </summary>
        /// <param name="projection">The current projection.</param>
        /// <returns>A new projection containing the appended buffer.</returns>
        public static IProjection As(this IProjection projection) => projection.Map(a => a.As());

        /// <summary>
        /// Appends the <c> AS </c> operator to the current attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute As(this IProjectionAttribute attribute) => attribute.Append(" AS ");

        /// <summary>
        /// Appends the <c> = </c> operator to the current attribute buffer.
        /// </summary>
        /// <param name="attribute">The current attribute.</param>
        /// <returns>A new attribute containing the appended buffer.</returns>
        public static IProjectionAttribute Eq(this IProjectionAttribute attribute) => attribute.Append(" = ");
    }
}
