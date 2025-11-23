using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace NutritionDiary.Views;

public partial class BarcodeScannerPage : ContentPage
{
    private string _selectedImagePath;
    public event Action<string> OnBarcodeScanned;

    public BarcodeScannerPage()
    {
        InitializeComponent();
    }

    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            // Вместо проверки IsSupported используем try-catch
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
            {
                await LoadPhoto(photo.FullPath);
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Ошибка", "Камера не поддерживается на этом устройстве", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Ошибка", "Не предоставлено разрешение на использование камеры", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сделать фото: {ex.Message}", "OK");
        }
    }

    private async void OnPickPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                await LoadPhoto(photo.FullPath);
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Ошибка", "Выбор фото не поддерживается на этом устройстве", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Ошибка", "Не предоставлено разрешение на доступ к фото", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
        }
    }

    private async Task LoadPhoto(string imagePath)
    {
        try
        {
            _selectedImagePath = imagePath;
            PreviewImage.Source = ImageSource.FromFile(imagePath);
            PreviewFrame.IsVisible = true;
            AnalyzeButton.IsVisible = true;

            await DisplayAlert("Успех", "Фото загружено. Теперь можно проанализировать штрих-код.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить фото: {ex.Message}", "OK");
        }
    }

    private async void OnAnalyzeClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedImagePath))
        {
            await DisplayAlert("Ошибка", "Сначала выберите фото со штрих-кодом", "OK");
            return;
        }

        try
        {
            // Показываем индикатор загрузки
            await DisplayAlert("Анализ", "Анализируем штрих-код...", "OK");

            // В реальном приложении здесь будет вызов API для распознавания штрих-кода
            // Пока используем заглушку
            var mockBarcode = await AnalyzeBarcodeMock(_selectedImagePath);

            if (!string.IsNullOrEmpty(mockBarcode))
            {
                OnBarcodeScanned?.Invoke(mockBarcode);
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Не найдено", "Не удалось распознать штрих-код на фото", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка анализа: {ex.Message}", "OK");
        }
    }

    // Заглушка для анализа штрих-кода
    private async Task<string> AnalyzeBarcodeMock(string imagePath)
    {
        string barcode = await DisplayPromptAsync(
            "Ручной ввод",
            "Автоматическое распознавание временно недоступно.\n\nВведите штрих-код вручную:",
            "OK",
            "Отмена",
            "5901234123457",
            maxLength: 20,
            keyboard: Keyboard.Numeric
        );

        return barcode;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}