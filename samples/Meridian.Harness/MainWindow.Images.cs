// ============================================================================
// Meridian
// File: MainWindow.Images.cs
// Author: Angel Hernandez (me@angelhernandezm.com)
// Description:
// The Images tab: generation, editing, and vision analysis -- the direct-.NET
// equivalent of Meridian.Interop.Com's IMeridianImageService. Generation and
// editing only work against a provider configured for images (OpenAI or Azure
// OpenAI); analysis works with any vision-capable chat model.
//
// License: MIT
// ============================================================================
//
// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ============================================================================

using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Meridian.Models.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Meridian.Harness;

partial class MainWindow {
    // ---- Generate ---------------------------------------------------------------
    private readonly TextBox _generatePrompt = new() { AcceptsReturn = true, Height = 50, Watermark = "Generation prompt" };
    private readonly NumericUpDown _generateWidth = new() { Value = 1024, Minimum = 256, Maximum = 2048, Increment = 64, Width = 100 };
    private readonly NumericUpDown _generateHeight = new() { Value = 1024, Minimum = 256, Maximum = 2048, Increment = 64, Width = 100 };
    private readonly Image _generatePreview = new() { Stretch = Avalonia.Media.Stretch.Uniform, MaxHeight = 280, HorizontalAlignment = HorizontalAlignment.Left };
    private readonly TextBox _imagesStatus = Output(60);
    private GeneratedImage? _lastGenerated;

    // ---- Edit -----------------------------------------------------------------
    private readonly Image _editSourcePreview = new() { Stretch = Avalonia.Media.Stretch.Uniform, MaxHeight = 160, HorizontalAlignment = HorizontalAlignment.Left };
    private readonly Image _editResultPreview = new() { Stretch = Avalonia.Media.Stretch.Uniform, MaxHeight = 160, HorizontalAlignment = HorizontalAlignment.Left };
    private readonly TextBox _editPrompt = new() { AcceptsReturn = true, Height = 50, Watermark = "Edit instructions" };
    private readonly NumericUpDown _editWidth = new() { Value = 1024, Minimum = 256, Maximum = 2048, Increment = 64, Width = 100 };
    private readonly NumericUpDown _editHeight = new() { Value = 1024, Minimum = 256, Maximum = 2048, Increment = 64, Width = 100 };
    private byte[]? _editSourceBytes;
    private GeneratedImage? _lastEdited;

    // ---- Analyze --------------------------------------------------------------
    private readonly Image _analyzePreview = new() { Stretch = Avalonia.Media.Stretch.Uniform, MaxHeight = 160, HorizontalAlignment = HorizontalAlignment.Left };
    private readonly TextBox _analyzePrompt = new() { Watermark = "Question about the image" };
    private readonly TextBox _analyzeOutput = Output(80);
    private byte[]? _analyzeSourceBytes;
    private string _analyzeMediaType = "image/png";

    /// <summary>
    /// Builds the Images tab.
    /// </summary>
    /// <returns>The configured <see cref="TabItem" />.</returns>
    private TabItem BuildImagesTab() {
        var root = Stack(14);
        root.Children.Add(Heading("Images"));
        root.Children.Add(Help("Requires \"Images\" enabled on the Setup tab. Generation/editing need OpenAI or Azure OpenAI as the image provider; analysis works with any vision-capable chat model."));

        root.Children.Add(Heading("Generate"));
        root.Children.Add(_generatePrompt);
        var generateSizeRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        generateSizeRow.Children.Add(new TextBlock { Text = "Width:", VerticalAlignment = VerticalAlignment.Center });
        generateSizeRow.Children.Add(_generateWidth);
        generateSizeRow.Children.Add(new TextBlock { Text = "Height:", VerticalAlignment = VerticalAlignment.Center });
        generateSizeRow.Children.Add(_generateHeight);
        root.Children.Add(generateSizeRow);
        var generateRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var generateButton = new Button { Content = "Generate" };
        generateButton.Click += (_, _) => RunSafely(generateButton, _imagesStatus, "Generating...", GenerateAsync);
        generateRow.Children.Add(generateButton);
        var saveGeneratedButton = new Button { Content = "Save As..." };
        saveGeneratedButton.Click += (_, _) => RunSafely(saveGeneratedButton, _imagesStatus, "Saving...", () => SaveImageAsync(_lastGenerated));
        generateRow.Children.Add(saveGeneratedButton);
        root.Children.Add(generateRow);
        root.Children.Add(_generatePreview);

        root.Children.Add(new Separator());
        root.Children.Add(Heading("Edit"));
        var loadSourceButton = new Button { Content = "Load source image..." };
        loadSourceButton.Click += (_, _) => RunSafely(loadSourceButton, _imagesStatus, "Loading...", LoadEditSourceAsync);
        root.Children.Add(loadSourceButton);
        root.Children.Add(_editSourcePreview);
        root.Children.Add(_editPrompt);
        var editSizeRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        editSizeRow.Children.Add(new TextBlock { Text = "Width:", VerticalAlignment = VerticalAlignment.Center });
        editSizeRow.Children.Add(_editWidth);
        editSizeRow.Children.Add(new TextBlock { Text = "Height:", VerticalAlignment = VerticalAlignment.Center });
        editSizeRow.Children.Add(_editHeight);
        root.Children.Add(editSizeRow);
        var editRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var editButton = new Button { Content = "Edit" };
        editButton.Click += (_, _) => RunSafely(editButton, _imagesStatus, "Editing...", EditAsync);
        editRow.Children.Add(editButton);
        var saveEditedButton = new Button { Content = "Save As..." };
        saveEditedButton.Click += (_, _) => RunSafely(saveEditedButton, _imagesStatus, "Saving...", () => SaveImageAsync(_lastEdited));
        editRow.Children.Add(saveEditedButton);
        root.Children.Add(editRow);
        root.Children.Add(_editResultPreview);

        root.Children.Add(new Separator());
        root.Children.Add(Heading("Analyze (vision)"));
        var loadAnalyzeButton = new Button { Content = "Load image..." };
        loadAnalyzeButton.Click += (_, _) => RunSafely(loadAnalyzeButton, _imagesStatus, "Loading...", LoadAnalyzeSourceAsync);
        root.Children.Add(loadAnalyzeButton);
        root.Children.Add(_analyzePreview);
        root.Children.Add(_analyzePrompt);
        var analyzeButton = new Button { Content = "Analyze" };
        analyzeButton.Click += (_, _) => RunSafely(analyzeButton, _analyzeOutput, "Analyzing...", AnalyzeAsync);
        root.Children.Add(analyzeButton);
        root.Children.Add(_analyzeOutput);

        root.Children.Add(_imagesStatus);
        return new TabItem { Header = "Images", Content = Page(root) };
    }

    private async Task GenerateAsync() {
        if (!RequireServices(_imagesStatus))
            return;

        if (string.IsNullOrWhiteSpace(_generatePrompt.Text)) {
            _imagesStatus.Text = "Enter a generation prompt first.";
            return;
        }

        var images = _services!.GetRequiredService<IMeridianImageService>();
        var width = (int)(_generateWidth.Value ?? 1024);
        var height = (int)(_generateHeight.Value ?? 1024);
        var result = await images.GenerateAsync(_generatePrompt.Text, width, height).ConfigureAwait(true);

        _lastGenerated = result;
        _generatePreview.Source = await LoadBitmapAsync(result.Data).ConfigureAwait(true);
        _imagesStatus.Text = $"Generated {result.Data.Length:N0} bytes ({result.MediaType}).";
    }

    private async Task LoadEditSourceAsync() {
        var bytes = await PickImageFileAsync("Select an image to edit").ConfigureAwait(true);
        if (bytes is null)
            return;

        _editSourceBytes = bytes;
        _editSourcePreview.Source = await LoadBitmapAsync(bytes).ConfigureAwait(true);
        _imagesStatus.Text = $"Loaded source image ({bytes.Length:N0} bytes).";
    }

    private async Task EditAsync() {
        if (!RequireServices(_imagesStatus))
            return;

        if (_editSourceBytes is null) {
            _imagesStatus.Text = "Load a source image first.";
            return;
        }
        if (string.IsNullOrWhiteSpace(_editPrompt.Text)) {
            _imagesStatus.Text = "Enter edit instructions first.";
            return;
        }

        var images = _services!.GetRequiredService<IMeridianImageService>();
        var width = (int)(_editWidth.Value ?? 1024);
        var height = (int)(_editHeight.Value ?? 1024);
        var result = await images.EditAsync(_editSourceBytes, _editPrompt.Text, null, width, height).ConfigureAwait(true);

        _lastEdited = result;
        _editResultPreview.Source = await LoadBitmapAsync(result.Data).ConfigureAwait(true);
        _imagesStatus.Text = $"Edited image: {result.Data.Length:N0} bytes ({result.MediaType}).";
    }

    private async Task LoadAnalyzeSourceAsync() {
        var bytes = await PickImageFileAsync("Select an image to analyze", reportMediaType: mediaType => _analyzeMediaType = mediaType).ConfigureAwait(true);
        if (bytes is null)
            return;

        _analyzeSourceBytes = bytes;
        _analyzePreview.Source = await LoadBitmapAsync(bytes).ConfigureAwait(true);
        _imagesStatus.Text = $"Loaded image to analyze ({bytes.Length:N0} bytes).";
    }

    private async Task AnalyzeAsync() {
        if (!RequireServices(_analyzeOutput))
            return;

        if (_analyzeSourceBytes is null) {
            _analyzeOutput.Text = "Load an image first.";
            return;
        }
        if (string.IsNullOrWhiteSpace(_analyzePrompt.Text)) {
            _analyzeOutput.Text = "Enter a question about the image first.";
            return;
        }

        var images = _services!.GetRequiredService<IMeridianImageService>();
        var response = await images.AnalyzeAsync(_analyzeSourceBytes, _analyzePrompt.Text, _analyzeMediaType).ConfigureAwait(true);
        _analyzeOutput.Text = response;
    }

    /// <summary>
    /// Opens a file picker restricted to images and returns the selected file's raw bytes, or
    /// <see langword="null" /> if the user cancelled.
    /// </summary>
    /// <param name="title">The file picker dialog's title.</param>
    /// <param name="reportMediaType">An optional callback invoked with a best-guess media type derived from the file extension.</param>
    /// <returns>The selected file's bytes, or <see langword="null" /> if cancelled.</returns>
    private async Task<byte[]?> PickImageFileAsync(string title, Action<string>? reportMediaType = null) {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        }).ConfigureAwait(true);

        if (files.Count == 0)
            return null;

        reportMediaType?.Invoke(GuessMediaType(files[0].Name));

        await using var stream = await files[0].OpenReadAsync().ConfigureAwait(true);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer).ConfigureAwait(true);
        return buffer.ToArray();
    }

    /// <summary>
    /// Opens a save-file picker and writes <paramref name="image" />'s bytes to the chosen path.
    /// </summary>
    /// <param name="image">The image to save, or <see langword="null" /> if none has been produced yet.</param>
    private async Task SaveImageAsync(GeneratedImage? image) {
        if (image is null) {
            _imagesStatus.Text = "Nothing to save yet.";
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        var extension = image.MediaType.Contains("jpeg") ? "jpg" : image.MediaType.Contains("webp") ? "webp" : "png";
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = "Save image",
            SuggestedFileName = $"meridian-image.{extension}",
            DefaultExtension = extension
        }).ConfigureAwait(true);

        if (file is null)
            return;

        await using var stream = await file.OpenWriteAsync().ConfigureAwait(true);
        await stream.WriteAsync(image.Data).ConfigureAwait(true);
        _imagesStatus.Text = $"Saved to {file.Name}.";
    }

    /// <summary>
    /// Decodes <paramref name="bytes" /> into an Avalonia <see cref="Bitmap" /> for display in an
    /// <see cref="Image" /> control.
    /// </summary>
    /// <param name="bytes">The encoded image bytes (PNG/JPEG/WebP).</param>
    /// <returns>The decoded bitmap.</returns>
    private static async Task<Bitmap> LoadBitmapAsync(byte[] bytes) {
        using var stream = new MemoryStream(bytes);
        return await Task.Run(() => new Bitmap(stream)).ConfigureAwait(true);
    }

    /// <summary>
    /// Guesses a media type from a file name's extension, defaulting to <c>image/png</c> when unrecognized.
    /// </summary>
    /// <param name="fileName">The file name to inspect.</param>
    /// <returns>The guessed media type.</returns>
    private static string GuessMediaType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        ".bmp" => "image/bmp",
        _ => "image/png"
    };
}
