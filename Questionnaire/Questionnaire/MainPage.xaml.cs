using System.Text;

namespace Questionnaire;

public partial class MainPage : ContentPage
{
	// Исходные значения, к которым возвращает кнопка «Очистить» (совпадают с XAML)
	private static readonly DateTime DefaultVisitDate = new(2026, 10, 1);
	private const double DefaultRating = 7;

	public MainPage()
	{
		InitializeComponent();
	}

	// Вызывается при вводе имени/email и выборе напитка:
	// кнопка «Показать результат» активна только когда заполнены обязательные поля.
	private void OnFormChanged(object? sender, EventArgs e)
	{
		ShowResultButton.IsEnabled = !string.IsNullOrWhiteSpace(NameEntry.Text)
		                             && DrinkPicker.SelectedIndex >= 0;
	}

	// Поле email показывается только тем, кто хочет получать рассылку
	private void OnNewsletterToggled(object? sender, ToggledEventArgs e)
	{
		EmailEntry.IsVisible = e.Value;
		if (!e.Value)
			EmailEntry.Text = string.Empty;
	}

	private async void OnShowResultClicked(object? sender, EventArgs e)
	{
		var errors = new List<string>();

		if (string.IsNullOrWhiteSpace(NameEntry.Text))
			errors.Add("• укажите имя");
		if (DrinkPicker.SelectedIndex < 0)
			errors.Add("• выберите любимый напиток");
		if (NewsletterSwitch.IsToggled && !IsValidEmail(EmailEntry.Text))
			errors.Add("• введите корректный email для рассылки");

		if (errors.Count > 0)
		{
			ResultBorder.IsVisible = false;
			await DisplayAlertAsync("Анкета не заполнена", string.Join("\n", errors), "ОК");
			return;
		}

		var summary = new StringBuilder();
		summary.AppendLine("Спасибо за отзыв! Ваши ответы:");
		summary.AppendLine();
		summary.AppendLine($"Имя: {NameEntry.Text.Trim()}");
		summary.AppendLine($"Рассылка: {(NewsletterSwitch.IsToggled ? EmailEntry.Text.Trim() : "не нужна")}");
		summary.AppendLine($"Любимый напиток: {DrinkPicker.SelectedItem}");
		summary.AppendLine($"Частота визитов: {GetVisitFrequency()}");
		summary.AppendLine($"Дата визита: {VisitDatePicker.Date:dd.MM.yyyy}");
		summary.AppendLine($"Оценка обслуживания: {RatingSlider.Value:F0} из 10");
		summary.AppendLine($"Брал(а) десерт: {(DessertCheckBox.IsChecked ? "да" : "нет")}");
		summary.AppendLine($"Посоветует друзьям: {(RecommendCheckBox.IsChecked ? "да" : "нет")}");
		summary.Append($"Отзыв: {(string.IsNullOrWhiteSpace(ReviewEditor.Text) ? "—" : ReviewEditor.Text.Trim())}");

		ResultLabel.Text = summary.ToString();
		ResultBorder.IsVisible = true;
	}

	private void OnClearClicked(object? sender, EventArgs e)
	{
		NameEntry.Text = string.Empty;
		NewsletterSwitch.IsToggled = false; // через OnNewsletterToggled скроет и очистит email
		DrinkPicker.SelectedIndex = -1;

		VisitFirstRadio.IsChecked = false;
		VisitMonthlyRadio.IsChecked = false;
		VisitWeeklyRadio.IsChecked = false;

		VisitDatePicker.Date = DefaultVisitDate;
		RatingSlider.Value = DefaultRating;
		DessertCheckBox.IsChecked = false;
		RecommendCheckBox.IsChecked = true;
		ReviewEditor.Text = string.Empty;

		ResultLabel.Text = string.Empty;
		ResultBorder.IsVisible = false;
		ShowResultButton.IsEnabled = false;
	}

	private string GetVisitFrequency()
	{
		RadioButton[] radios = [VisitFirstRadio, VisitMonthlyRadio, VisitWeeklyRadio];
		return radios.FirstOrDefault(r => r.IsChecked)?.Value?.ToString() ?? "не указано";
	}

	private static bool IsValidEmail(string? email) =>
		!string.IsNullOrWhiteSpace(email)
		&& email.IndexOf('@') > 0
		&& email.LastIndexOf('.') > email.IndexOf('@') + 1;
}
