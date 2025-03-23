using UnityEngine;
using UnityEngine.UIElements;

namespace AstralAvarice.Frontend
{
    public class IncomePieChartController
    {
		private float goal;
		private float income;

		private const string PIE_CHART_ELEMENT_NAME = "PieChart";
		private const string INCOME_LABEL_ELEMENT_NAME = "PieChartIncome";
		private const string GOAL_LABEL_ELEMENT_NAME = "PieChartGoal";

		private VisualElement pieChartElement;
		private Label pieChartIncomeLabel;
		private Label pieChartGoalLabel;

		public void Bind(VisualElement rootElement)
		{
			if (pieChartElement != null)
				pieChartElement.generateVisualContent -= DrawPieChart;

			pieChartElement = rootElement.Q(PIE_CHART_ELEMENT_NAME);
			pieChartElement.generateVisualContent += DrawPieChart;
			pieChartIncomeLabel = rootElement.Q<Label>(INCOME_LABEL_ELEMENT_NAME);
			pieChartGoalLabel = rootElement.Q<Label>(GOAL_LABEL_ELEMENT_NAME);

			Update();
		}

		public void SetIncomeAndGoal(float income, float goal)
		{
			Debug.Assert(goal > 0, $"Set pie chart drawer to draw an income goal of {goal}. The income goal must always be positive and > 0.");

			if (this.income == income && this.goal == goal)
				return;

			this.income = income;
			this.goal = goal;

			Update();
		}

		private void Update()
		{
			pieChartIncomeLabel.text = income.ToString();
			pieChartGoalLabel.text = goal.ToString();

			// Invoke calling DrawPieChart via generateVisualContent.
			pieChartElement.MarkDirtyRepaint();
		}

		private void DrawPieChart(MeshGenerationContext ctx)
		{
			// TODO: Handle case where goal == 0?

			Vector2 center = new Vector2(pieChartElement.resolvedStyle.width / 2, pieChartElement.resolvedStyle.height / 2);
			float radius = Mathf.Min(pieChartElement.resolvedStyle.width, pieChartElement.resolvedStyle.height) / 2;

			IncomePieChartDrawingUtils.DrawPieChart(
				ctx,
				income,
				goal,
				center,
				radius,
				Color.green,
				Color.red
			);
		}
    }
}
