using Godot;
using ProjectSilicon.Scripts.World.Belts;
using ProjectSilicon.Scripts.World.Machines;
using ProjectSilicon.Scripts.World.Resources;

namespace ProjectSilicon.Scripts.World.Build;

public partial class BuildLayer
{
	// Stores the selected cell and keeps its details updated while simulation runs.
	private void ShowDetailsObject(Vector2I cell)
	{
		_inspectedCell = cell;
		UpdateObjectDetailsLabel();
	}

	private void UpdateObjectDetailsLabel()
	{
		if (ObjectDetailsLabel is null)
			return;

		if (_inspectedCell is not Vector2I cell)
		{
			ObjectDetailsLabel.Visible = false;
			return;
		}

		if (_machines.TryGetValue(cell, out MachineInstance? machine))
		{
			DrawMachineDetails(machine);
			return;
		}

		if (_belts.TryGetValue(cell, out BeltInstance? belt))
		{
			DrawBeltDetails(belt);
			return;
		}

		ObjectDetailsLabel.Visible = false;
		_inspectedCell = null;
	}

	private void DrawMachineDetails(MachineInstance machine)
	{
		if (ObjectDetailsLabel is null)
			return;

		MachineDefinition definition =
			MachineDatabase.Get(machine.Type);

		RecipeDefinition recipe =
			definition.Recipe;

		double progress =
			recipe.Duration > 0
				? machine.ProductionProgress / recipe.Duration
				: 0.0;

		string inputText = "Empty";

		if (recipe.InputResource is ResourceType inputResource)
		{
			inputText =
				$"{inputResource}: " +
				$"{machine.GetInputAmount(inputResource)}/{machine.MaxInput}";
		}

		string outputText =
			$"{recipe.OutputResource}: " +
			$"{machine.GetOutputAmount(recipe.OutputResource)}/{machine.MaxOutput}";

		ObjectDetailsLabel.Text = $"""
								  {definition.Name}
								  Producing: {machine.IsProducing}
								  Progress: {progress:P0}
								  Input:
								  {inputText}
								  Output:
								  {outputText}
								  """;

		PositionDetailsLabel();
		ObjectDetailsLabel.Visible = true;
	}

	private void DrawBeltDetails(BeltInstance belt)
	{
		if (ObjectDetailsLabel is null)
			return;

		string itemText =
			belt.CurrentItem?.ToString() ?? "Empty";

		ObjectDetailsLabel.Text = $"""
								   Belt
								   Direction: {belt.BeltDirection}
								   Item: {itemText}
								   Progress: {belt.ItemProgress:P0}
								   Speed: {belt.Speed:0.##}
								   """;

		PositionDetailsLabel();
		ObjectDetailsLabel.Visible = true;
	}

	private void PositionDetailsLabel()
	{
		if (ObjectDetailsLabel is null)
			return;

		Vector2 mousePosition =
			GetViewport().GetMousePosition();

		ObjectDetailsLabel.Position =
			mousePosition + new Vector2(8, 8);
	}
}