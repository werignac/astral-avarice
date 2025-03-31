using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using AstralAvarice.Utils;

namespace AstralAvarice.Frontend
{
    public static class IncomePieChartDrawingUtils
    {
		private const float UP_ANGLE = 270f;

		public static void DrawPieChart(
			MeshGenerationContext ctx,
			float income,
			float goal,
			Vector2 center,
			float radius,
			Color incomeColor,
			Color goalColor)
		{
			float percent = Mathf.Clamp01(income / goal);
			float incomeHalfAngle = percent * 360f / 2;

			Painter2D painter = ctx.painter2D;

			painter.lineWidth = 0f;

			if (percent > 0 && percent < 1)
			{
				painter.fillColor = incomeColor;
				painter.DrawCircleSlice(UP_ANGLE - incomeHalfAngle, UP_ANGLE + incomeHalfAngle, center, radius);
				painter.fillColor = goalColor;
				painter.DrawCircleSlice(UP_ANGLE + incomeHalfAngle, UP_ANGLE - incomeHalfAngle, center, radius);
			}
			else
			{
				painter.fillColor = (percent == 0) ? goalColor : incomeColor;
				painter.DrawCircle(center, radius);
			}
		}
    }
}
