using RayMmdNodeEditor.Controls;
using RayMmdNodeEditor.Graph;
using RayMmdNodeEditor.Services;

namespace RayMmdNodeEditor;

public sealed class MainForm : Form
{
    private readonly NodeCanvas _canvas = new();
    private readonly RayMaterialCompiler _compiler = new();
    private readonly RayAdvancedMaterialCompiler _advancedCompiler = new();
    private readonly SplitContainer _mainSplit = new();
    private readonly TabControl _rightTabs = new();
    private readonly Panel _inspectorPanel = new();
    private readonly Panel _settingsPanel = new();
    private readonly ListBox _problemListBox = new();
    private readonly ComboBox _previewFileCombo = new();
    private readonly RichTextBox _previewTextBox = new();
    private readonly ToolStripStatusLabel _statusLabel = new();
    private readonly TextBox _rayRootBox = new();
    private readonly TextBox _exportDirectoryBox = new();
    private readonly TextBox _settingsSearchBox = new();
    private readonly CheckBox _settingsFavoritesOnlyBox = new();
    private readonly System.Windows.Forms.Timer _autoExportTimer = new()
    {
        Interval = 750,
    };
    private readonly ToolTip _toolTip = new()
    {
        AutoPopDelay = 12000,
        InitialDelay = 350,
        ReshowDelay = 100,
        ShowAlways = true,
    };

    private RayDocument _document = new();
    private string? _documentPath;
    private bool _isRefreshingInspector;
    private bool _isUpdatingModeCombo;
    private bool _isRefreshingSettingsPanel;
    private bool _isAutoExporting;
    private readonly Dictionary<string, string> _previewFiles = new(StringComparer.Ordinal);
    private readonly List<RayProblemItem> _problemItems = [];

    private sealed record ChoiceItem(string Label, string Value)
    {
        public override string ToString() => Label;
    }

    private sealed record RayProblemItem(string Severity, string Message, Guid? NodeId)
    {
        public override string ToString() => $"[{Severity}] {Message}";
    }

    private sealed class WheelSafeComboBox : ComboBox
    {
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (DroppedDown)
            {
                base.OnMouseWheel(e);
                return;
            }

            if (e is HandledMouseEventArgs handled)
            {
                handled.Handled = true;
            }
        }
    }

    public MainForm()
    {
        Text = "Ray-MMD 材质节点编辑器";
        Width = 1360;
        Height = 860;
        MinimumSize = new Size(1100, 680);
        StartPosition = FormStartPosition.CenterScreen;
        Font = SystemFonts.MessageBoxFont;

        InitializeLayout();
        _autoExportTimer.Tick += (_, _) =>
        {
            _autoExportTimer.Stop();
            RunAutoExport();
        };
        FormClosed += (_, _) => _autoExportTimer.Dispose();
        BindDocument(new RayDocument(), null);
    }

    private void InitializeLayout()
    {
        var menu = BuildMenu();
        Controls.Add(menu);
        MainMenuStrip = menu;

        var status = new StatusStrip { Dock = DockStyle.Bottom };
        status.Items.Add(_statusLabel);
        Controls.Add(status);

        _canvas.NodeKindFilter = RayGraphFactory.IsRayNodeAllowed;
        _canvas.FilterDescription = "Ray-MMD";
        _canvas.NodeBadgeProvider = node => RayNodeSupport.GetBadge(node.Kind, IsAdvancedMode());
        _canvas.SearchGroupLabelProvider = definition => RayNodeSupport.GetSearchGroup(definition.Kind, IsAdvancedMode());
        _canvas.GraphChanged += (_, _) => RefreshPreview();
        _canvas.SelectedNodeChanged += (_, _) => RefreshInspector();

        _mainSplit.Dock = DockStyle.Fill;
        _mainSplit.SplitterWidth = 6;
        _mainSplit.Panel1.Controls.Add(_canvas);
        Controls.Add(_mainSplit);
        _mainSplit.BringToFront();

        _rightTabs.Dock = DockStyle.Fill;
        _mainSplit.Panel2.Controls.Add(_rightTabs);

        var inspectorTab = new TabPage("属性") { BackColor = SystemColors.Control };
        var settingsTab = new TabPage("Ray 参数") { BackColor = SystemColors.Control };
        var problemsTab = new TabPage("问题") { BackColor = SystemColors.Control };
        var previewTab = new TabPage("FX 预览") { BackColor = SystemColors.Control };
        _rightTabs.TabPages.Add(inspectorTab);
        _rightTabs.TabPages.Add(settingsTab);
        _rightTabs.TabPages.Add(problemsTab);
        _rightTabs.TabPages.Add(previewTab);

        _inspectorPanel.Dock = DockStyle.Fill;
        _inspectorPanel.AutoScroll = true;
        _inspectorPanel.Padding = new Padding(12);
        inspectorTab.Controls.Add(_inspectorPanel);

        _settingsPanel.Dock = DockStyle.Fill;
        _settingsPanel.AutoScroll = true;
        _settingsPanel.Padding = new Padding(12);
        settingsTab.Controls.Add(_settingsPanel);
        _settingsSearchBox.TextChanged += (_, _) =>
        {
            if (!_isRefreshingSettingsPanel)
            {
                RefreshSettingsPanel();
            }
        };
        _settingsFavoritesOnlyBox.CheckedChanged += (_, _) =>
        {
            if (!_isRefreshingSettingsPanel)
            {
                RefreshSettingsPanel();
            }
        };

        _problemListBox.Dock = DockStyle.Fill;
        _problemListBox.IntegralHeight = false;
        _problemListBox.Font = new Font(Font.FontFamily, 9f);
        _problemListBox.DoubleClick += (_, _) => SelectProblemNode();
        _problemListBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectProblemNode();
                e.Handled = true;
            }
        };
        problemsTab.Controls.Add(_problemListBox);

        _previewTextBox.Dock = DockStyle.Fill;
        _previewTextBox.ReadOnly = true;
        _previewTextBox.WordWrap = false;
        _previewTextBox.ScrollBars = RichTextBoxScrollBars.Both;
        _previewTextBox.Font = new Font("Consolas", 10f);
        _previewFileCombo.Dock = DockStyle.Top;
        _previewFileCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _previewFileCombo.SelectedIndexChanged += (_, _) =>
        {
            if (_previewFileCombo.SelectedItem is string key && _previewFiles.TryGetValue(key, out var content))
            {
                _previewTextBox.Text = content;
            }
        };
        previewTab.Controls.Add(_previewTextBox);
        previewTab.Controls.Add(_previewFileCombo);

        Shown += (_, _) =>
        {
            _mainSplit.SplitterDistance = Math.Max(650, Width - 470);
        };
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip { Dock = DockStyle.Top };
        var file = new ToolStripMenuItem("文件");
        file.DropDownItems.Add("新建", null, (_, _) => BindDocument(new RayDocument(), null));
        file.DropDownItems.Add("打开...", null, (_, _) => OpenDocument());
        file.DropDownItems.Add("保存", null, (_, _) => SaveDocument());
        file.DropDownItems.Add("另存为...", null, (_, _) => SaveDocumentAs());
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("导出 Ray 预设", null, (_, _) => ExportPreset());
        file.DropDownItems.Add("单独导出材质 FX...", null, (_, _) => ExportMaterialFxOnly());
        menu.Items.Add(file);

        var help = new ToolStripMenuItem("帮助");
        help.DropDownItems.Add("Ray 模式说明", null, (_, _) => ShowHelpDialog());
        menu.Items.Add(help);

        return menu;
    }

    private void BindDocument(RayDocument document, string? path)
    {
        _document = document;
        _documentPath = path;
        _canvas.Graph = _document.Graph;
        RefreshSettingsPanel();
        RefreshInspector();
        RefreshPreview();
        UpdateTitle();
    }

    private void UpdateTitle()
    {
        var name = string.IsNullOrWhiteSpace(_documentPath) ? "未命名" : Path.GetFileName(_documentPath);
        Text = $"Ray-MMD 材质节点编辑器 - {name}";
    }

    private void OpenDocument()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Ray-MMD Graph (*.raymmdgraph.json)|*.raymmdgraph.json|JSON (*.json)|*.json|All Files (*.*)|*.*",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        BindDocument(RayDocumentPersistence.Load(dialog.FileName), dialog.FileName);
    }

    private void SaveDocument()
    {
        if (string.IsNullOrWhiteSpace(_documentPath))
        {
            SaveDocumentAs();
            return;
        }

        _document.Graph = _canvas.Graph;
        RayDocumentPersistence.Save(_document, _documentPath);
        SetStatus("已保存节点图。");
    }

    private void SaveDocumentAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Ray-MMD Graph (*.raymmdgraph.json)|*.raymmdgraph.json|JSON (*.json)|*.json",
            FileName = "ray-material.raymmdgraph.json",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _documentPath = dialog.FileName;
        SaveDocument();
        UpdateTitle();
    }

    private void ExportPreset()
    {
        if (TryExportPresetCore(interactive: true, copyFullPackage: true, out var message))
        {
            SetStatus(message);
        }
    }

    private bool TryExportPresetCore(bool interactive, bool copyFullPackage, out string message)
    {
        message = string.Empty;

        RefreshDocumentPaths();
        var featureAnalysis = RayFeatureAnalyzer.Analyze(_document, applyAutoEnable: true);
        var result = _compiler.Compile(_canvas.Graph);
        var advancedResult = IsAdvancedMode() ? _advancedCompiler.Compile(_document) : null;
        var visibleCompileMessages = GetVisibleCompileMessages(result.Messages);
        if (IsAdvancedMode())
        {
            result = result with
            {
                Success = result.Success || visibleCompileMessages.Count == 0,
                Messages = visibleCompileMessages,
            };
        }

        if (!result.Success && visibleCompileMessages.Count > 0)
        {
            return FailExport(interactive, string.Join(Environment.NewLine, result.Messages), "无法导出", out message);
        }

        var rayRoot = _document.RayRootPath;
        var exportRoot = _document.ExportDirectory;
        var materialFileName = RayExportNaming.GetMaterialFileName(_document);
        _document.MaterialFileName = materialFileName;
        var sourceRayConf = Path.Combine(rayRoot, "ray.conf");
        var sourceAdvancedConf = Path.Combine(rayRoot, "ray_advanced.conf");
        var sourceCommon = Path.Combine(rayRoot, "Materials", "material_common_2.0.fxsub");
        if (!File.Exists(sourceRayConf) || !File.Exists(sourceAdvancedConf) || !File.Exists(sourceCommon))
        {
            return FailExport(interactive, "Ray 根目录必须包含 ray.conf、ray_advanced.conf 和 Materials/material_common_2.0.fxsub。", "Ray 根目录无效", out message);
        }

        var materialDir = Path.Combine(exportRoot, "Materials");
        Directory.CreateDirectory(materialDir);
        if (interactive && !ConfirmExportPlan(exportRoot, materialDir))
        {
            message = "已取消导出。";
            return false;
        }

        var packageResult = copyFullPackage
            ? RayPackageExportManager.Export(_document)
            : new RayPackageExportResult(false, 0, [], []);
        if (packageResult.Warnings.Any(warning => warning.Contains("must not be inside", StringComparison.OrdinalIgnoreCase)))
        {
            return FailExport(interactive, string.Join(Environment.NewLine, packageResult.Warnings), "整包导出失败", out message);
        }
        Directory.CreateDirectory(materialDir);

        if (advancedResult is { Success: false })
        {
            return FailExport(interactive, string.Join(Environment.NewLine, advancedResult.Messages), "高级节点模式无法导出", out message);
        }

        var compatibilityIssues = RayCompatibilityChecker.Check(_document, result.MaterialText, advancedResult);
        if (interactive && compatibilityIssues.Count > 0)
        {
            var answer = MessageBox.Show(
                this,
                "兼容性检查发现问题：" + Environment.NewLine + Environment.NewLine +
                string.Join(Environment.NewLine, compatibilityIssues.Select(issue => "- " + issue)) +
                Environment.NewLine + Environment.NewLine +
                "仍然继续导出吗？",
                "Ray 兼容性检查",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes)
            {
                message = "已取消导出。";
                return false;
            }
        }

        var materialCommon = RayConfigWriter.ApplyMaterialCommonDefines(
            File.ReadAllText(sourceCommon),
            _document.MaterialCommonValues);
        if (advancedResult is not null)
        {
            materialCommon = RayAdvancedCommonPatcher.Patch(materialCommon, advancedResult);
        }

        var textureResult = RayTextureExportManager.CopyTexturesAndRewriteMaterial(_document, result.MaterialText, materialCommon, exportRoot);
        var lightingResult = RayAdvancedLightingPatcher.ExportDirectionalLightPatch(_document, exportRoot);
        var shadingResult = RayAdvancedShadingPatcher.Export(_document, exportRoot, advancedResult);
        File.WriteAllText(Path.Combine(materialDir, materialFileName), textureResult.MaterialText);
        File.WriteAllText(Path.Combine(materialDir, "material_common_2.0.fxsub"), textureResult.CommonText);

        var rayConf = RayConfigWriter.ApplyDefines(File.ReadAllText(sourceRayConf), _document.RayConfValues);
        var advancedConf = RayConfigWriter.ApplyStaticConstFloats(File.ReadAllText(sourceAdvancedConf), _document.AdvancedConfValues);
        File.WriteAllText(Path.Combine(exportRoot, "ray.conf"), rayConf);
        File.WriteAllText(Path.Combine(exportRoot, "ray_advanced.conf"), advancedConf);
        RayExportReportWriter.Write(exportRoot, _document, result, textureResult, compatibilityIssues, advancedResult, lightingResult, shadingResult, featureAnalysis, packageResult);

        var warningCount = compatibilityIssues.Count +
                           packageResult.Warnings.Count +
                           textureResult.Warnings.Count +
                           lightingResult.Warnings.Count +
                           shadingResult.Warnings.Count;
        message = warningCount == 0
            ? $"已导出 Ray 预设到 {exportRoot}"
            : $"已导出 Ray 预设到 {exportRoot}（{warningCount} 条提示/警告）";
        return true;
    }

    private bool FailExport(bool interactive, string text, string title, out string message)
    {
        message = text;
        if (interactive)
        {
            MessageBox.Show(this, text, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        return false;
    }

    private void ExportMaterialFxOnly()
    {
        RefreshDocumentPaths();
        var result = _compiler.Compile(_canvas.Graph);
        var advancedResult = IsAdvancedMode() ? _advancedCompiler.Compile(_document) : null;
        var visibleCompileMessages = GetVisibleCompileMessages(result.Messages);
        if (IsAdvancedMode())
        {
            result = result with
            {
                Success = result.Success || visibleCompileMessages.Count == 0,
                Messages = visibleCompileMessages,
            };
        }

        if (!result.Success && visibleCompileMessages.Count > 0)
        {
            MessageBox.Show(this, string.Join(Environment.NewLine, visibleCompileMessages), "无法导出 FX", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (advancedResult is { Success: false })
        {
            MessageBox.Show(this, string.Join(Environment.NewLine, advancedResult.Messages), "高级节点模式无法导出 FX", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (advancedResult is not null &&
            !string.IsNullOrWhiteSpace(advancedResult.ShadingPatchBlock))
        {
            var answer = MessageBox.Show(
                this,
                "当前高级节点图需要配套 patched material_common_2.0.fxsub 或 Shader 文件。单独导出 FX 只会写材质 .fx 文件，完整效果请使用“导出 Ray 预设”。仍然继续吗？",
                "单独导出 FX",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (answer != DialogResult.Yes)
            {
                return;
            }
        }

        var fileName = RayExportNaming.GetMaterialFileName(_document);
        using var dialog = new SaveFileDialog
        {
            Filter = "Ray Material FX (*.fx)|*.fx|All Files (*.*)|*.*",
            FileName = fileName,
            DefaultExt = "fx",
            AddExtension = true,
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var materialDirectory = Path.GetDirectoryName(dialog.FileName);
        if (string.IsNullOrWhiteSpace(materialDirectory))
        {
            MessageBox.Show(this, "请选择有效的 FX 导出路径。", "无法导出 FX", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Directory.CreateDirectory(materialDirectory);
        var sourceCommon = Path.Combine(_document.RayRootPath, "Materials", "material_common_2.0.fxsub");
        if (!File.Exists(sourceCommon))
        {
            MessageBox.Show(this, $"找不到 material_common_2.0.fxsub：{sourceCommon}", "无法导出 FX", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var materialCommon = RayConfigWriter.ApplyMaterialCommonDefines(
            File.ReadAllText(sourceCommon),
            _document.MaterialCommonValues);
        if (advancedResult is not null)
        {
            materialCommon = RayAdvancedCommonPatcher.Patch(materialCommon, advancedResult);
        }

        var textureResult = RayTextureExportManager.CopyTexturesAndRewriteMaterialAndCommonToDirectory(_document, result.MaterialText, materialCommon, materialDirectory);
        File.WriteAllText(dialog.FileName, textureResult.MaterialText);
        File.WriteAllText(Path.Combine(materialDirectory, "material_common_2.0.fxsub"), textureResult.CommonText);
        if (textureResult.Warnings.Count > 0)
        {
            MessageBox.Show(this, string.Join(Environment.NewLine, textureResult.Warnings), "FX 已导出，但有贴图警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        SetStatus($"已单独导出材质 FX 到 {dialog.FileName}");
    }

    private bool ConfirmExportPlan(string exportRoot, string materialDir)
    {
        var fixedFiles = new[]
        {
            Path.Combine(materialDir, RayExportNaming.GetMaterialFileName(_document)),
            Path.Combine(materialDir, "material_common_2.0.fxsub"),
            Path.Combine(exportRoot, "ray.conf"),
            Path.Combine(exportRoot, "ray_advanced.conf"),
            Path.Combine(exportRoot, "RayMmdNodeEditor_Report.txt"),
        };

        var lines = new List<string>
        {
            "即将导出 Ray 预设：",
            exportRoot,
            string.Empty,
            _document.ExportFullRayPackage
                ? "将先复制完整 Ray 包到导出目录，然后覆盖写入节点生成/patch 的文件。"
                : "将只写入节点生成/patch 的必要文件。若目标目录不是完整 Ray 包，可能缺少运行依赖。",
            string.Empty,
            "将写入文件：",
        };
        foreach (var path in fixedFiles)
        {
            lines.Add($"{(File.Exists(path) ? "覆盖" : "新建")} - {path}");
        }

        var fileTextureNodes = _document.Graph.Nodes
            .Where(node => node.Kind == NodeKind.RayTextureSlot)
            .Where(node => RayCompatibilityChecker.IsFileSource(ReadNodeProperty(node, "Source", "File")))
            .ToList();
        if (_document.CopyTextureFiles && fileTextureNodes.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("将复制的文件贴图：");
            foreach (var node in fileTextureNodes)
            {
                var file = ReadNodeProperty(node, "File", string.Empty);
                var absolute = RayCompatibilityChecker.ResolveTexturePath(_document, file);
                lines.Add($"{(absolute is not null && File.Exists(absolute) ? "复制" : "缺失")} - {file}");
            }
        }

        lines.Add(string.Empty);
        lines.Add("原始 Ray 根目录不会被修改。继续导出吗？");
        return MessageBox.Show(
            this,
            string.Join(Environment.NewLine, lines),
            "导出确认",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Information) == DialogResult.Yes;
    }

    private void RefreshInspector()
    {
        if (_isRefreshingInspector)
        {
            return;
        }

        _isRefreshingInspector = true;
        try
        {
            _inspectorPanel.SuspendLayout();
            _inspectorPanel.Controls.Clear();
            var node = _canvas.SelectedNode;
            if (node is null)
            {
                AddInspectorLabel("选择一个节点后，可以在这里编辑 Ray 参数。", bold: false);
                return;
            }

            var definition = NodeRegistry.Get(node.Kind);
            AddInspectorLabel(UiTextHelper.NodeTitle(definition), bold: true);
            AddInspectorLabel(UiTextHelper.NodeDescription(definition), bold: false);
            AddInspectorInfo(
                RayNodeSupport.GetBadge(node.Kind, IsAdvancedMode()),
                RayNodeSupport.GetDescription(node.Kind, IsAdvancedMode()));
            foreach (var property in definition.Properties)
            {
                AddPropertyEditor(node, property);
            }

            if (node.Kind == NodeKind.RayTextureSlot)
            {
                AddTexturePreview(node);
            }
        }
        finally
        {
            _inspectorPanel.ResumeLayout();
            _isRefreshingInspector = false;
        }
    }

    private void AddInspectorLabel(string text, bool bold)
    {
        var label = new Label
        {
            Dock = DockStyle.Top,
            Height = bold ? 28 : 40,
            Text = text,
            Font = bold ? new Font(Font, FontStyle.Bold) : Font,
        };
        _inspectorPanel.Controls.Add(label);
        label.BringToFront();
    }

    private void AddInspectorInfo(string title, string text)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(245, 248, 252),
        };
        var titleLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 20,
            Text = title,
            Font = new Font(Font, FontStyle.Bold),
        };
        var bodyLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = text,
        };
        panel.Controls.Add(bodyLabel);
        panel.Controls.Add(titleLabel);
        _inspectorPanel.Controls.Add(panel);
        panel.BringToFront();
    }

    private void AddPropertyEditor(GraphNode node, NodePropertyDefinition property)
    {
        var row = new Panel { Dock = DockStyle.Top, Height = property.Editor == NodePropertyEditorKind.FilePath ? 72 : 56 };
        var label = new Label { Dock = DockStyle.Top, Height = 20, Text = GetPropertyLabel(property) };
        var currentValue = node.Properties.TryGetValue(property.Name, out var value) ? value : property.DefaultValue;
        var choices = GetPropertyChoices(property.Name);

        if (choices.Length > 0)
        {
            var combo = CreateChoiceCombo(choices, currentValue);
            combo.SelectedIndexChanged += (_, _) =>
            {
                if (combo.SelectedItem is ChoiceItem item)
                {
                    node.Properties[property.Name] = item.Value;
                    _canvas.Invalidate();
                    RefreshPreview();
                }
            };
            row.Controls.Add(combo);
            row.Controls.Add(label);
            _inspectorPanel.Controls.Add(row);
            row.BringToFront();
            return;
        }

        var box = new TextBox { Dock = DockStyle.Top, Text = currentValue };
        box.TextChanged += (_, _) =>
        {
            node.Properties[property.Name] = box.Text;
            _canvas.Invalidate();
            RefreshPreview();
        };

        if (property.Editor == NodePropertyEditorKind.FilePath)
        {
            var browse = new Button { Dock = DockStyle.Top, Height = 26, Text = "选择贴图..." };
            browse.Click += (_, _) =>
            {
                using var dialog = new OpenFileDialog
                {
                    Filter = "Texture Files (*.png;*.jpg;*.jpeg;*.bmp;*.tga;*.dds)|*.png;*.jpg;*.jpeg;*.bmp;*.tga;*.dds|All Files (*.*)|*.*",
                    FileName = box.Text,
                };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    box.Text = dialog.FileName;
                }
            };
            row.Controls.Add(browse);
        }

        row.Controls.Add(box);
        row.Controls.Add(label);
        _inspectorPanel.Controls.Add(row);
        row.BringToFront();
    }

    private void AddTexturePreview(GraphNode node)
    {
        RefreshDocumentPaths();
        var source = ReadNodeProperty(node, "Source", "File");
        var file = ReadNodeProperty(node, "File", string.Empty);
        var absolute = RayCompatibilityChecker.ResolveTexturePath(_document, file);
        var exists = absolute is not null && File.Exists(absolute);
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 210,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(248, 248, 248),
        };
        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 22,
            Text = "贴图预览",
            Font = new Font(Font, FontStyle.Bold),
        };
        var info = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 54,
            Text = BuildTextureInfoText(source, file, absolute, exists),
        };
        var picture = new PictureBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(232, 234, 238),
            SizeMode = PictureBoxSizeMode.Zoom,
        };

        if (exists && absolute is not null)
        {
            try
            {
                using var stream = File.OpenRead(absolute);
                using var image = Image.FromStream(stream);
                var bitmap = new Bitmap(image);
                picture.Image = bitmap;
                picture.Disposed += (_, _) => bitmap.Dispose();
                info.Text = BuildTextureInfoText(source, file, absolute, exists, image.Width, image.Height, image.PixelFormat.ToString());
            }
            catch (Exception ex)
            {
                info.Text = BuildTextureInfoText(source, file, absolute, exists) + Environment.NewLine + $"读取失败：{ex.Message}";
            }
        }

        panel.Controls.Add(picture);
        panel.Controls.Add(info);
        panel.Controls.Add(title);
        _inspectorPanel.Controls.Add(panel);
        panel.BringToFront();
    }

    private static string BuildTextureInfoText(string source, string file, string? absolute, bool exists, int? width = null, int? height = null, string? format = null)
    {
        var status = RayCompatibilityChecker.IsFileSource(source)
            ? exists ? "文件存在" : "找不到文件"
            : "非文件来源，不复制贴图";
        var size = width is null || height is null ? string.Empty : $"，{width} x {height}";
        var pixel = string.IsNullOrWhiteSpace(format) ? string.Empty : $"，{format}";
        return $"{status}{size}{pixel}{Environment.NewLine}{file}{Environment.NewLine}{absolute ?? "未解析到路径"}";
    }

    private static ComboBox CreateChoiceCombo(IReadOnlyList<ChoiceItem> choices, string currentValue)
    {
        var combo = new WheelSafeComboBox
        {
            Dock = DockStyle.Top,
            DropDownStyle = ComboBoxStyle.DropDownList,
        };

        var items = choices.ToList();
        var selected = items.FirstOrDefault(item =>
            string.Equals(item.Value, currentValue, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(item.Label, currentValue, StringComparison.OrdinalIgnoreCase));
        if (selected is null && !string.IsNullOrWhiteSpace(currentValue))
        {
            selected = new ChoiceItem(currentValue, currentValue);
            items.Add(selected);
        }

        combo.Items.AddRange(items.Cast<object>().ToArray());
        combo.SelectedItem = selected ?? items.FirstOrDefault();
        return combo;
    }

    private static string GetPropertyLabel(NodePropertyDefinition property)
    {
        if (!string.IsNullOrWhiteSpace(property.DisplayName) && property.DisplayName.Any(ch => ch > 127))
        {
            return property.DisplayName;
        }

        return property.Name switch
        {
            "Value" => "数值",
            "Type" => "数据类型",
            "Min" => "最小值",
            "Max" => "最大值",
            "Exponent" => "指数",
            "T" => "插值比例",
            "Start" => "输入起点",
            "End" => "输入终点",
            "Mode" => "模式",
            "Channels" => "通道",
            "InMin" => "输入最小值",
            "InMax" => "输入最大值",
            "OutMin" => "输出最小值",
            "OutMax" => "输出最大值",
            "Gamma" => "伽马",
            "Exposure" => "曝光",
            "HueShift" => "色相偏移",
            "Temperature" => "色温",
            "Tint" => "色调",
            "Contrast" => "对比度",
            "Saturation" => "饱和度",
            "ShadowLift" => "阴影抬升",
            "HighlightCompress" => "高光压缩",
            "Lift" => "Lift 抬升",
            "Gain" => "Gain 增益",
            "LayerMode" => "混合模式",
            "MaskInvert" => "反转遮罩",
            "GradientType" => "渐变类型",
            "Dimensions" => "维度",
            "Scale" => "缩放",
            "WaveType" => "波纹类型",
            "WaveProfile" => "波形",
            "Distortion" => "扭曲",
            "SpeedU" => "U 速度",
            "SpeedV" => "V 速度",
            "Angle" => "角度",
            "CenterU" => "中心 U",
            "CenterV" => "中心 V",
            "ScaleU" => "U 缩放",
            "ScaleV" => "V 缩放",
            "OffsetU" => "U 偏移",
            "OffsetV" => "V 偏移",
            "StartR" => "起始红色 R",
            "StartG" => "起始绿色 G",
            "StartB" => "起始蓝色 B",
            "StartA" => "起始透明 A",
            "EndR" => "结束红色 R",
            "EndG" => "结束绿色 G",
            "EndB" => "结束蓝色 B",
            "EndA" => "结束透明 A",
            "R" => "红色 R",
            "G" => "绿色 G",
            "B" => "蓝色 B",
            "A" => "透明 A",
            "X" => "X",
            "Y" => "Y",
            "Z" => "Z",
            "W" => "W",
            _ => string.IsNullOrWhiteSpace(property.DisplayName) ? property.Name : property.DisplayName,
        };
    }

    private void RefreshSettingsPanel()
    {
        _isRefreshingSettingsPanel = true;
        _settingsPanel.SuspendLayout();
        _settingsPanel.Controls.Clear();

        AddPathEditor("Ray 根目录", _document.RayRootPath, value => _document.RayRootPath = value, _rayRootBox);
        AddPathEditor("导出目录", _document.ExportDirectory, value => _document.ExportDirectory = value, _exportDirectoryBox);
        AddWorkflowOptions();
        AddParameterFilterBar();
        AddSettingsGroup("ray.conf", _document.RayConfValues);
        foreach (var group in RayConfigDefaults.CreateAdvancedGroups())
        {
            AddSettingsGroup(group.Title, _document.AdvancedConfValues, group.Keys);
        }
        AddSettingsGroup("material_common_2.0.fxsub 可选补丁", _document.MaterialCommonValues);

        AddSettingsGroup("Lighting / DirectionalLight 可选补丁", _document.LightingPatchValues);

        _settingsPanel.ResumeLayout();
        _isRefreshingSettingsPanel = false;
    }

    private void AddParameterFilterBar()
    {
        var row = new Panel { Dock = DockStyle.Top, Height = 92, Padding = new Padding(0, 3, 0, 9) };
        var separator = CreateRowSeparator();
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));

        var label = new Label { Dock = DockStyle.Fill, Text = "参数过滤", TextAlign = ContentAlignment.MiddleLeft };
        var description = new Label
        {
            Dock = DockStyle.Fill,
            Text = "按参数名或说明搜索；星标收藏常调参数，方便后续只看重点项。",
            ForeColor = SystemColors.GrayText,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.TopLeft,
        };
        _toolTip.SetToolTip(description, description.Text);

        _settingsSearchBox.Dock = DockStyle.Fill;
        _settingsSearchBox.PlaceholderText = "搜索 Ray 参数，例如 bloom、shadow、alpha...";
        _settingsFavoritesOnlyBox.Dock = DockStyle.Fill;
        _settingsFavoritesOnlyBox.Text = "只看收藏";
        _settingsFavoritesOnlyBox.TextAlign = ContentAlignment.MiddleLeft;

        layout.Controls.Add(label, 0, 0);
        layout.SetColumnSpan(label, 2);
        layout.Controls.Add(description, 0, 1);
        layout.SetColumnSpan(description, 2);
        layout.Controls.Add(_settingsSearchBox, 0, 2);
        layout.Controls.Add(_settingsFavoritesOnlyBox, 1, 2);

        row.Controls.Add(separator);
        row.Controls.Add(layout);
        _settingsPanel.Controls.Add(row);
        row.BringToFront();
    }

    private void AddWorkflowOptions()
    {
        var titleLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = "导出工作流",
            Font = new Font(Font, FontStyle.Bold),
        };
        _settingsPanel.Controls.Add(titleLabel);
        titleLabel.BringToFront();

        var presetChoices = RayQualityPresets.Choices
            .Select(item => new ChoiceItem(item.Label, item.Value))
            .ToArray();
        var presetRow = CreateSimpleSettingRow(
            "质量预设",
            "一键调整 ray.conf 和部分 advanced 参数；之后仍可手动细调。",
            CreateChoiceCombo(presetChoices, _document.QualityPreset));
        if (presetRow.Editor is ComboBox presetCombo)
        {
            presetCombo.SelectedIndexChanged += (_, _) =>
            {
                if (presetCombo.SelectedItem is not ChoiceItem item)
                {
                    return;
                }

                _document.QualityPreset = item.Value;
                RayQualityPresets.Apply(item.Value, _document);
                RefreshSettingsPanel();
                RefreshPreview();
            };
        }
        _settingsPanel.Controls.Add(presetRow.Row);
        presetRow.Row.BringToFront();

        var modeChoices = new[]
        {
            new ChoiceItem("兼容模式", RayMaterialModes.Compatible),
            new ChoiceItem("高级节点模式", RayMaterialModes.Advanced),
        };
        var modeRow = CreateSimpleSettingRow(
            "材质模式",
            "兼容模式保持 Ray 原版结构；高级节点模式会 patch 导出副本的 material_common_2.0.fxsub。",
            CreateChoiceCombo(modeChoices, _document.MaterialMode));
        if (modeRow.Editor is ComboBox modeCombo)
        {
            modeCombo.SelectedIndexChanged += (_, _) =>
            {
                if (_isUpdatingModeCombo)
                {
                    return;
                }

                if (modeCombo.SelectedItem is ChoiceItem item)
                {
                    if (string.Equals(item.Value, RayMaterialModes.Compatible, StringComparison.OrdinalIgnoreCase) &&
                        !ConfirmCompatibleModeSwitch(modeCombo))
                    {
                        return;
                    }

                    _document.MaterialMode = item.Value;
                    _canvas.Invalidate();
                    RefreshInspector();
                    RefreshPreview();
                }
            };
        }
        _settingsPanel.Controls.Add(modeRow.Row);
        modeRow.Row.BringToFront();

        var materialNameBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = RayExportNaming.GetMaterialFileName(_document),
        };
        materialNameBox.Leave += (_, _) =>
        {
            _document.MaterialFileName = RayExportNaming.NormalizeMaterialFileName(materialNameBox.Text);
            materialNameBox.Text = _document.MaterialFileName;
            RefreshPreview();
        };
        materialNameBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            _document.MaterialFileName = RayExportNaming.NormalizeMaterialFileName(materialNameBox.Text);
            materialNameBox.Text = _document.MaterialFileName;
            RefreshPreview();
            e.SuppressKeyPress = true;
        };
        var materialNameRow = CreateSimpleSettingRow(
            "材质 FX 文件名",
            "导出到 Materials 目录里的节点材质文件名；可写 my_skin.fx。include 仍指向同目录 material_common_2.0.fxsub。",
            materialNameBox);
        _settingsPanel.Controls.Add(materialNameRow.Row);
        materialNameRow.Row.BringToFront();

        var exportFxButton = new Button
        {
            Dock = DockStyle.Fill,
            Text = "单独导出材质 FX...",
        };
        exportFxButton.Click += (_, _) => ExportMaterialFxOnly();
        var exportFxRow = CreateSimpleSettingRow(
            "单独导出 FX",
            "只导出当前节点生成的材质 .fx 文件；若开启贴图路径管理，会把文件贴图复制到同目录 textures。",
            exportFxButton);
        _settingsPanel.Controls.Add(exportFxRow.Row);
        exportFxRow.Row.BringToFront();

        var copyBox = new CheckBox
        {
            Dock = DockStyle.Fill,
            Text = "导出时复制文件贴图到 Materials/textures，并改写为相对路径",
            Checked = _document.CopyTextureFiles,
        };
        copyBox.CheckedChanged += (_, _) =>
        {
            _document.CopyTextureFiles = copyBox.Checked;
            RefreshPreview();
        };
        var copyRow = CreateSimpleSettingRow(
            "贴图路径管理",
            "只处理 Ray 贴图槽里 Source=文件贴图 的路径；PMX 主贴图/Sphere/Toon 不会复制。",
            copyBox);
        _settingsPanel.Controls.Add(copyRow.Row);
        copyRow.Row.BringToFront();

        var packageBox = new CheckBox
        {
            Dock = DockStyle.Fill,
            Text = "导出时复制完整 Ray 包，再写入节点生成文件和 patch",
            Checked = _document.ExportFullRayPackage,
        };
        packageBox.CheckedChanged += (_, _) =>
        {
            _document.ExportFullRayPackage = packageBox.Checked;
            RefreshPreview();
        };
        var packageRow = CreateSimpleSettingRow(
            "整包导出",
            "开启后会把 Ray 根目录完整复制到导出目录，避免缺少 Shader、Extension、Lighting、Skybox 等依赖；原始 Ray 不会被修改。",
            packageBox);
        _settingsPanel.Controls.Add(packageRow.Row);
        packageRow.Row.BringToFront();

        var autoExportBox = new CheckBox
        {
            Dock = DockStyle.Fill,
            Text = "改动后自动增量导出到当前导出目录（750ms 防抖）",
            Checked = _document.AutoExportEnabled,
        };
        autoExportBox.CheckedChanged += (_, _) =>
        {
            _document.AutoExportEnabled = autoExportBox.Checked;
            if (_document.AutoExportEnabled)
            {
                ScheduleAutoExport();
                SetStatus("自动导出已开启；请先确认 Ray 根目录和导出目录正确。");
            }
            else
            {
                _autoExportTimer.Stop();
                SetStatus("自动导出已关闭。");
            }
        };
        var autoExportRow = CreateSimpleSettingRow(
            "自动导出",
            "用于在 MMD 中伪实时查看变化：只覆盖节点生成的 FX、conf 和 patch 文件，不重复复制完整 Ray 包；首次建议先手动“导出 Ray 预设”。",
            autoExportBox);
        _settingsPanel.Controls.Add(autoExportRow.Row);
        autoExportRow.Row.BringToFront();
    }

    private (Panel Row, Control Editor) CreateSimpleSettingRow(string labelText, string descriptionText, Control editor)
    {
        var rowWidth = Math.Max(260, _settingsPanel.ClientSize.Width - 36);
        var descriptionHeight = MeasureWrappedTextHeight(descriptionText, rowWidth);
        var row = new Panel { Dock = DockStyle.Top, Height = descriptionHeight + 68, Padding = new Padding(0, 3, 0, 9) };
        var separator = CreateRowSeparator();
        var layout = CreateSettingLayout(descriptionHeight);
        var label = new Label { Dock = DockStyle.Fill, Text = labelText, TextAlign = ContentAlignment.MiddleLeft };
        var description = new Label
        {
            Dock = DockStyle.Fill,
            Text = descriptionText,
            ForeColor = SystemColors.GrayText,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.TopLeft,
        };
        _toolTip.SetToolTip(description, descriptionText);
        editor.Dock = DockStyle.Fill;
        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(description, 0, 1);
        layout.Controls.Add(editor, 0, 2);
        row.Controls.Add(separator);
        row.Controls.Add(layout);
        return (row, editor);
    }

    private bool ConfirmCompatibleModeSwitch(ComboBox modeCombo)
    {
        var advancedNodes = _canvas.Graph.Nodes
            .Where(node => RayNodeSupport.RequiresAdvancedMode(node.Kind))
            .Select(node => NodeRegistry.Get(node.Kind).Title)
            .Distinct()
            .ToList();
        if (advancedNodes.Count == 0)
        {
            return true;
        }

        var answer = MessageBox.Show(
            this,
            "当前节点图包含仅高级模式生效的节点：" + Environment.NewLine +
            string.Join(Environment.NewLine, advancedNodes.Select(name => "- " + name)) +
            Environment.NewLine + Environment.NewLine +
            "切回兼容模式后，这些节点不会导出逐像素效果。仍然切换吗？",
            "切换到兼容模式",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer == DialogResult.Yes)
        {
            return true;
        }

        _isUpdatingModeCombo = true;
        try
        {
            foreach (var item in modeCombo.Items.OfType<ChoiceItem>())
            {
                if (string.Equals(item.Value, _document.MaterialMode, StringComparison.OrdinalIgnoreCase))
                {
                    modeCombo.SelectedItem = item;
                    break;
                }
            }
        }
        finally
        {
            _isUpdatingModeCombo = false;
        }

        return false;
    }

    private void AddPathEditor(string labelText, string value, Action<string> setter, TextBox targetBox)
    {
        var row = new Panel { Dock = DockStyle.Top, Height = 88, Padding = new Padding(0, 0, 0, 5) };
        var separator = CreateRowSeparator();
        var label = new Label { Dock = DockStyle.Top, Height = 20, Text = labelText };
        var browse = new Button { Dock = DockStyle.Top, Height = 26, Text = "选择文件夹..." };
        targetBox.Dock = DockStyle.Top;
        targetBox.Text = value;
        targetBox.TextChanged += (_, _) =>
        {
            setter(targetBox.Text);
            RefreshPreview();
        };
        browse.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = labelText,
                SelectedPath = Directory.Exists(targetBox.Text) ? targetBox.Text : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                targetBox.Text = dialog.SelectedPath;
            }
        };
        _toolTip.SetToolTip(targetBox, labelText == "Ray 根目录"
            ? "请选择包含 ray.conf、ray_advanced.conf 和 Materials/material_common_2.0.fxsub 的 Ray-MMD 根目录。"
            : "导出会写入这个副本目录，不会覆盖原始 ray-mmd。");
        row.Controls.Add(targetBox);
        row.Controls.Add(browse);
        row.Controls.Add(label);
        row.Controls.Add(separator);
        _settingsPanel.Controls.Add(row);
        row.BringToFront();
    }

    private void AddSettingsGroup(string title, Dictionary<string, string> values, IReadOnlyList<string>? keys = null)
    {
        var visibleKeys = (keys ?? values.Keys.ToList())
            .Where(values.ContainsKey)
            .Where(key => IsRaySettingVisible(title, key))
            .ToList();
        if (visibleKeys.Count == 0)
        {
            return;
        }

        var titleLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = title,
            Font = new Font(Font, FontStyle.Bold),
        };
        _settingsPanel.Controls.Add(titleLabel);
        titleLabel.BringToFront();

        foreach (var key in visibleKeys)
        {
            var descriptionText = GetRaySettingDescription(key);
            var rowWidth = Math.Max(260, _settingsPanel.ClientSize.Width - 36);
            var descriptionHeight = MeasureWrappedTextHeight(descriptionText, rowWidth);
            var row = new Panel { Dock = DockStyle.Top, Height = descriptionHeight + 68, Padding = new Padding(0, 3, 0, 9) };
            var separator = CreateRowSeparator();
            var layout = CreateSettingLayout(descriptionHeight);

            var label = new Label
            {
                Dock = DockStyle.Fill,
                Text = key,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            var description = new Label
            {
                Dock = DockStyle.Fill,
                Height = descriptionHeight,
                Text = descriptionText,
                ForeColor = SystemColors.GrayText,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft,
            };
            _toolTip.SetToolTip(description, description.Text);
            var choices = GetRaySettingChoices(key);
            Control editor;
            if (choices.Length > 0)
            {
                var combo = CreateChoiceCombo(choices, values[key]);
                combo.SelectedIndexChanged += (_, _) =>
                {
                    if (combo.SelectedItem is ChoiceItem item)
                    {
                        values[key] = item.Value;
                        RefreshPreview();
                    }
                };
                editor = combo;
            }
            else
            {
                var box = new TextBox { Text = values[key] };
                box.TextChanged += (_, _) =>
                {
                    values[key] = box.Text.Trim();
                    RefreshPreview();
                };
                editor = box;
            }

            editor.Dock = DockStyle.Fill;
            var favorite = new CheckBox
            {
                Appearance = Appearance.Button,
                Dock = DockStyle.Right,
                Width = 34,
                Text = IsFavoriteRayParameter(key) ? "★" : "☆",
                TextAlign = ContentAlignment.MiddleCenter,
                Checked = IsFavoriteRayParameter(key),
            };
            _toolTip.SetToolTip(favorite, "收藏这个参数");
            favorite.CheckedChanged += (_, _) =>
            {
                SetFavoriteRayParameter(key, favorite.Checked);
                favorite.Text = favorite.Checked ? "★" : "☆";
                if (_settingsFavoritesOnlyBox.Checked)
                {
                    RefreshSettingsPanel();
                }
            };
            var editorHost = new Panel { Dock = DockStyle.Fill };
            editorHost.Controls.Add(editor);
            editorHost.Controls.Add(favorite);
            layout.Controls.Add(label, 0, 0);
            layout.Controls.Add(description, 0, 1);
            layout.Controls.Add(editorHost, 0, 2);
            row.Controls.Add(separator);
            row.Controls.Add(layout);
            _settingsPanel.Controls.Add(row);
            row.BringToFront();
        }
    }

    private bool IsRaySettingVisible(string groupTitle, string key)
    {
        if (_settingsFavoritesOnlyBox.Checked && !IsFavoriteRayParameter(key))
        {
            return false;
        }

        var query = _settingsSearchBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var description = GetRaySettingDescription(key);
        return ContainsIgnoreCase(key, query) ||
               ContainsIgnoreCase(groupTitle, query) ||
               ContainsIgnoreCase(description, query);
    }

    private bool IsFavoriteRayParameter(string key)
    {
        return _document.FavoriteParameterKeys.Any(item => string.Equals(item, key, StringComparison.OrdinalIgnoreCase));
    }

    private void SetFavoriteRayParameter(string key, bool favorite)
    {
        _document.FavoriteParameterKeys.RemoveAll(item => string.Equals(item, key, StringComparison.OrdinalIgnoreCase));
        if (favorite)
        {
            _document.FavoriteParameterKeys.Add(key);
            _document.FavoriteParameterKeys.Sort(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static bool ContainsIgnoreCase(string text, string query)
    {
        return text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static TableLayoutPanel CreateSettingLayout(int descriptionHeight)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 18f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, descriptionHeight));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
        return layout;
    }

    private static Panel CreateRowSeparator()
    {
        return new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 1,
            BackColor = Color.FromArgb(215, 215, 215),
        };
    }

    private static ChoiceItem[] GetPropertyChoices(string propertyName)
    {
        return propertyName switch
        {
            "Slot" =>
            [
                new("主颜色 Albedo", "Albedo"),
                new("副颜色 SubAlbedo", "SubAlbedo"),
                new("透明度 Alpha", "Alpha"),
                new("法线 Normal", "Normal"),
                new("副法线 SubNormal", "SubNormal"),
                new("光滑度 Smoothness", "Smoothness"),
                new("金属度 Metalness", "Metalness"),
                new("高光 Specular", "Specular"),
                new("遮蔽 Occlusion", "Occlusion"),
                new("视差 Parallax", "Parallax"),
                new("自发光 Emissive", "Emissive"),
                new("自定义 A CustomA", "CustomA"),
                new("自定义 B CustomB", "CustomB"),
            ],
            "Source" =>
            [
                new("无贴图", "None"),
                new("文件贴图", "File"),
                new("动画贴图", "Animated"),
                new("PMX 主贴图", "PMXTexture"),
                new("PMX Sphere", "PMXSphere"),
                new("PMX Toon", "PMXToon"),
                new("Dummy Screen", "DummyScreen"),
                new("PMX Ambient", "PMXAmbient"),
                new("PMX Specular", "PMXSpecular"),
                new("PMX Specular Power", "PMXSpecularPower"),
            ],
            "UvFlip" =>
            [
                new("不翻转", "None"),
                new("翻转 X", "X"),
                new("翻转 Y", "Y"),
                new("翻转 X 与 Y", "XY"),
            ],
            "Swizzle" or "AlphaMapSwizzle" =>
            [
                new("R 通道", "R"),
                new("G 通道", "G"),
                new("B 通道", "B"),
                new("A 通道", "A"),
            ],
            "MapType" =>
            [
                new("默认", "Default"),
                new("Roughness 转 Smoothness", "Roughness"),
                new("线性 Roughness 转 Smoothness", "LinearRoughness"),
                new("压缩法线", "CompressedNormal"),
                new("Bump 低质量", "BumpLQ"),
                new("Bump 高质量", "BumpHQ"),
                new("世界法线", "WorldNormal"),
                new("UE4 高光贴图", "SpecularUE4"),
                new("Frostbite 高光贴图", "SpecularFrostbite"),
                new("灰度 UE4 高光", "SpecularGrayUE4"),
                new("灰度 Frostbite 高光", "SpecularGrayFrostbite"),
                new("固定 0.4 高光", "SpecularFixed04"),
                new("线性遮蔽", "OcclusionLinear"),
                new("UV2 sRGB 遮蔽", "OcclusionUv2Srgb"),
                new("UV2 线性遮蔽", "OcclusionUv2Linear"),
                new("视差保留 Alpha", "ParallaxKeepAlpha"),
            ],
            "ApplyScale" =>
            [
                new("自动", "Auto"),
                new("关闭", "Off"),
                new("开启", "On"),
            ],
            "ColorFlip" =>
            [
                new("关闭", "Off"),
                new("开启", "On"),
            ],
            "Mode" =>
            [
                new("线性 Linear", "Linear"),
                new("平滑 Smooth", "Smooth"),
                new("硬切 Constant", "Constant"),
            ],
            "StopCount" =>
            [
                new("2 色标", "2"),
                new("3 色标", "3"),
                new("4 色标", "4"),
                new("5 色标", "5"),
            ],
            "LightSource" =>
            [
                new("Ray DirectionalLight.pmx", "RayDirectional"),
                new("自定义控制器", "Custom"),
                new("手动方向", "Manual"),
            ],
            "BlendMode" =>
            [
                new("乘色并加光", "MultiplyAdd"),
                new("只加光", "Add"),
                new("只乘色", "Multiply"),
            ],
            "ENABLE_DIRECTIONAL_LIGHTING_PATCH" =>
            [
                new("关闭", "0"),
                new("开启", "1"),
            ],
            "ENABLE_MULTI_LIGHTING_PATCH" =>
            [
                new("关闭", "0"),
                new("开启", "1"),
            ],
            "LIGHTING_PATCH_PRESET" =>
            [
                new("自定义", "Custom"),
                new("柔和卡通", "SoftToon"),
                new("强 Lambert", "StrongLambert"),
                new("保留 Ray 高光", "KeepHighlight"),
                new("反向补光", "FillLight"),
            ],
            "LIGHTING_PATCH_MODE" =>
            [
                new("Half Lambert 柔光", "HalfLambert"),
                new("Lambert 标准", "Lambert"),
                new("反向 Lambert", "Inverted"),
            ],
            "LIGHTING_PATCH_BLEND_SOURCE" or "LIGHTING_PATCH_SHADOW_SOURCE" or "LIGHTING_PATCH_SPECULAR_SOURCE" =>
            [
                new("常量", "Constant"),
                new("CustomA 节点桥", "CustomA"),
                new("Occlusion", "Occlusion"),
                new("Smoothness", "Smoothness"),
                new("Metalness", "Metalness"),
            ],
            "LIGHTING_PATCH_TINT_SOURCE" =>
            [
                new("常量", "Constant"),
                new("CustomB 节点桥", "CustomB"),
                new("Albedo", "Albedo"),
                new("Specular", "Specular"),
                new("Emissive", "Emissive"),
            ],
            "LayerMode" =>
            [
                new("正常 Normal", "Normal"),
                new("相乘 Multiply", "Multiply"),
                new("叠加 Overlay", "Overlay"),
                new("滤色 Screen", "Screen"),
                new("相加 Add", "Add"),
                new("相减 Subtract", "Subtract"),
                new("差值 Difference", "Difference"),
                new("变亮 Lighten", "Lighten"),
                new("变暗 Darken", "Darken"),
            ],
            "MaskInvert" =>
            [
                new("关闭", "False"),
                new("开启", "True"),
            ],
            "GradientType" =>
            [
                new("线性 Linear", "Linear"),
                new("径向 Radial", "Radial"),
                new("二次 Quadratic", "Quadratic"),
            ],
            "WaveType" =>
            [
                new("条纹 Bands", "Bands"),
                new("圆环 Rings", "Rings"),
            ],
            "WaveProfile" =>
            [
                new("正弦 Sine", "Sine"),
                new("锯齿 Saw", "Saw"),
                new("三角 Triangle", "Triangle"),
            ],
            "Dimensions" =>
            [
                new("2D", "2D"),
                new("3D", "3D"),
            ],
            "Channels" =>
            [
                new("RGBA", "RGBA"),
                new("RGB", "RGB"),
                new("R", "R"),
                new("G", "G"),
                new("B", "B"),
                new("A", "A"),
            ],
            "AlbedoApplyDiffuse" or "AlbedoApplyMorphColor" or "EmissiveEnabled" or "EmissiveApplyMorphColor" or "EmissiveApplyMorphIntensity" or "EmissiveApplyBlink" =>
            [
                new("开启", "On"),
                new("关闭", "Off"),
            ],
            "SubAlbedoMode" =>
            [
                new("禁用", "None"),
                new("相乘", "Multiply"),
                new("幂运算", "Power"),
                new("相加", "Add"),
                new("Melanin", "Melanin"),
                new("Alpha 混合", "AlphaBlend"),
            ],
            "CustomMode" =>
            [
                new("禁用", "None"),
                new("皮肤", "Skin"),
                new("自发光", "Emissive"),
                new("各向异性", "Anisotropy"),
                new("玻璃", "Glass"),
                new("布料", "Cloth"),
                new("清漆 Clear Coat", "ClearCoat"),
                new("次表面", "Subsurface"),
                new("卡通 Cel", "Cel"),
                new("Tone Based", "ToneBased"),
                new("遮罩", "Mask"),
            ],
            "BridgeMode" =>
            [
                new("Ray Native 原生管线", "RayNative"),
                new("Standalone 独立高光", "Standalone"),
            ],
            "RayModel" =>
            [
                new("Standard GGX", "StandardGGX"),
                new("Clear Coat", "ClearCoat"),
                new("Anisotropy", "Anisotropy"),
                new("Cloth", "Cloth"),
                new("Skin", "Skin"),
                new("Subsurface", "Subsurface"),
                new("Cel", "Cel"),
                new("Tone Based", "ToneBased"),
            ],
            _ => [],
        };
    }

    private static ChoiceItem[] GetRaySettingChoices(string key)
    {
        if (key is "TEXTURE_FILTER" or "TEXTURE_MIP_FILTER")
        {
            return
            [
                new("点采样 POINT", "POINT"),
                new("线性 LINEAR", "LINEAR"),
                new("各向异性 ANISOTROPIC", "ANISOTROPIC"),
            ];
        }

        if (key == "TEXTURE_ANISOTROPY_LEVEL")
        {
            return
            [
                new("1", "1"),
                new("2", "2"),
                new("4", "4"),
                new("8", "8"),
                new("16", "16"),
            ];
        }

        return key switch
        {
            "SUN_LIGHT_ENABLE" =>
            [
                new("0 关闭太阳光", "0"),
                new("1 启用太阳光", "1"),
                new("2 按太阳天顶角计算辐射", "2"),
            ],
            "SUN_SHADOW_QUALITY" =>
            [
                new("0 关闭阴影", "0"),
                new("1 低 512 x 4", "1"),
                new("2 中 1024 x 4", "2"),
                new("3 高 2048 x 4", "3"),
                new("4 超高 4096 x 4", "4"),
                new("5 极高 8192 x 4", "5"),
            ],
            "IBL_QUALITY" => [new("0 关闭", "0"), new("1 启用 IBL", "1"), new("2 启用 IBL + UV 翻转", "2")],
            "FOG_ENABLE" or "MULTI_LIGHT_ENABLE" or "SSSS_QUALITY" or "BOKEH_QUALITY" => [new("0 关闭", "0"), new("1 启用", "1")],
            "OUTLINE_QUALITY" =>
            [
                new("0 关闭描边", "0"),
                new("1 启用描边", "1"),
                new("2 描边 + SMAA", "2"),
                new("3 描边 + SSAA", "3"),
            ],
            "TOON_ENABLE" =>
            [
                new("0 关闭 Toon 材质支持", "0"),
                new("1 Toon 材质支持", "1"),
                new("2 Toon 材质 + diffusion", "2"),
            ],
            "SSDO_QUALITY" =>
            [
                new("0 关闭 SSDO", "0"),
                new("1 8 samples", "1"),
                new("2 12 samples", "2"),
                new("3 16 samples", "3"),
                new("4 20 samples", "4"),
                new("5 24 samples", "5"),
                new("6 28 samples", "6"),
            ],
            "SSR_QUALITY" => [new("0 关闭 SSR", "0"), new("1 32 samples", "1"), new("2 64 samples", "2"), new("3 128 samples", "3")],
            "HDR_EYE_ADAPTATION" =>
            [
                new("0 关闭自动曝光", "0"),
                new("1 ISO 100 / Middle Gray 12.7%", "1"),
                new("2 ISO 100 / Middle Gray 18.0%", "2"),
            ],
            "HDR_BLOOM_MODE" =>
            [
                new("0 关闭 Bloom", "0"),
                new("1 inf 旧版 Bloom", "1"),
                new("2 saturate", "2"),
                new("3 luminance + exposure", "3"),
                new("4 saturate + exposure 当前推荐", "4"),
            ],
            "HDR_FLARE_MODE" => [new("0 关闭 Flare", "0"), new("1 蓝色", "1"), new("2 橙色", "2"), new("3 自动", "3")],
            "HDR_STAR_MODE" =>
            [
                new("0 关闭星芒", "0"),
                new("1 蓝色横向镜头光", "1"),
                new("2 自动横向镜头光", "2"),
                new("3 橙色星芒", "3"),
                new("4 自动星芒", "4"),
            ],
            "HDR_TONEMAP_OPERATOR" =>
            [
                new("0 Linear 线性", "0"),
                new("1 Reinhard 保色亮度压缩", "1"),
                new("2 Hable filmic / white point 4", "2"),
                new("3 Uncharted2 filmic / white point 8", "3"),
                new("4 Hejl2015 默认推荐", "4"),
                new("5 ACES-sRGB 电影感", "5"),
                new("6 NaughtyDog", "6"),
            ],
            "AA_QUALITY" =>
            [
                new("0 关闭抗锯齿", "0"),
                new("1 FXAA", "1"),
                new("2 SMAA x1 medium", "2"),
                new("3 SMAA x1 high", "3"),
                new("4 SMAA x2 medium", "4"),
                new("5 SMAA x2 high", "5"),
            ],
            "POST_DISPERSION_MODE" =>
            [
                new("0 关闭色散", "0"),
                new("1 Color Shift 色彩偏移", "1"),
                new("2 Chromatic Aberration 色差", "2"),
            ],
            _ => [],
        };
    }

    private static string GetRaySettingDescription(string key)
    {
        return key switch
        {
            "SUN_LIGHT_ENABLE" => "0 关闭；1 启用；2 根据太阳天顶角计算太阳辐射。常用 1。",
            "SUN_SHADOW_QUALITY" => "0 关闭；1 512x4；2 1024x4；3 2048x4；4 4096x4；5 8192x4。越高越清晰也越慢。",
            "IBL_QUALITY" => "0 关闭；1 启用；2 启用并翻转 IBL UV。影响环境光和反射。",
            "FOG_ENABLE" => "0 关闭；1 启用。用于远景空气感。",
            "MULTI_LIGHT_ENABLE" => "0 关闭；1 启用。启用后可使用更多 Ray 辅助灯。",
            "OUTLINE_QUALITY" => "0 关闭；1 描边；2 描边+SMAA；3 描边+SSAA。",
            "TOON_ENABLE" => "0 关闭；1 Toon 材质支持；2 Toon 材质支持并带 diffusion。",
            "SSDO_QUALITY" => "0 关闭；1 8 samples；2 12 samples；3 16 samples；4 20 samples；5 24 samples；6 28 samples。",
            "SSR_QUALITY" => "0 关闭；1 32 samples；2 64 samples；3 128 samples。金属和光滑材质更明显。",
            "SSSS_QUALITY" => "0 关闭；1 启用。屏幕空间次表面散射，皮肤/半透材质常用。",
            "BOKEH_QUALITY" => "0 关闭；1 启用。景深散景；具体对焦模式由 Ray 控制器 MeasureMode 控制。",
            "HDR_EYE_ADAPTATION" => "0 关闭；1 ISO100 / Middle Gray 12.7%；2 ISO100 / Middle Gray 18.0%。",
            "HDR_BLOOM_MODE" => "0 关闭；1 inf 旧版；2 saturate；3 luminance+exposure；4 saturate+exposure，Ray 当前默认。",
            "HDR_FLARE_MODE" => "0 关闭；1 蓝色；2 橙色；3 自动。",
            "HDR_STAR_MODE" => "0 关闭；1 蓝色横向镜头光；2 自动横向镜头光；3 橙色星芒；4 自动星芒。",
            "HDR_TONEMAP_OPERATOR" => "0 Linear；1 Reinhard；2 Hable；3 Uncharted2；4 Hejl2015；5 ACES-sRGB；6 NaughtyDog。",
            "AA_QUALITY" => "0 关闭；1 FXAA；2 SMAA x1 medium；3 SMAA x1 high；4 SMAA x2 medium；5 SMAA x2 high。",
            "POST_DISPERSION_MODE" => "0 关闭；1 Color Shift 色彩偏移；2 Chromatic Aberration 色差。",
            "mLightIntensityMin" => "控制面板中主光强度的最小值。",
            "mLightIntensityMax" => "控制面板中主光强度的最大值。",
            "mLightDistance" => "主光距离/范围相关参数。",
            "mLightPlaneNear" => "主光投影近裁剪面；过小可能损失阴影精度。",
            "mLightPlaneFar" => "主光投影远裁剪面；影响阴影覆盖范围。",
            "mTemperature" => "默认色温，数值越低越暖，越高越冷。",
            "mPointLightNear" => "点光源近裁剪距离。",
            "mPointLightFar" => "点光源远裁剪距离。",
            "mEnvLightIntensityMin" => "环境光强度可调范围最小值。",
            "mEnvLightIntensityMax" => "环境光强度可调范围最大值。",
            "mBloomIntensityMin" => "Bloom 强度可调范围最小值。",
            "mBloomIntensityMax" => "Bloom 强度可调范围最大值。",
            "mBloomGhostThresholdMax" => "Bloom ghost/flare 阈值上限。",
            "mExposureMin" => "曝光可调范围最小值。",
            "mExposureMax" => "曝光可调范围最大值。",
            "mExposureEyeAdapationMin" => "自动曝光适应范围最小值。",
            "mExposureEyeAdapationMax" => "自动曝光适应范围最大值。",
            "mVignetteInner" => "暗角内圈范围。",
            "mVignetteOuter" => "暗角外圈范围。",
            "mPSSMCascadeZMin" => "级联阴影最近分段距离。",
            "mPSSMCascadeZMax" => "级联阴影最远分段距离。",
            "mPSSMCascadeLambda" => "级联阴影分布权重；影响近处/远处阴影精度分配。",
            "mPSSMDepthZMin" => "PSSM 深度最小范围。",
            "mPSSMDepthZMax" => "PSSM 深度最大范围。",
            "mSSRRangeMax" => "SSR 最大追踪距离。",
            "mSSRRangeScale" => "SSR 距离缩放；越高反射搜索越远。",
            "mSSRThreshold" => "SSR 命中阈值；影响反射稳定性。",
            "mSSRFadeStart" => "SSR 开始淡出的屏幕边缘范围。",
            "mSSSSIntensityMin" => "SSSS 强度最小值。",
            "mSSSSIntensityMax" => "SSSS 强度最大值。",
            "mSSDOParams" => "SSDO 参数数组 {半径/强度/阈值等}；保持大括号格式。",
            "mSSDOBiasNear" => "SSDO 近距离 bias，减少自遮蔽噪点。",
            "mSSDOBiasFar" => "SSDO 远距离 bias。",
            "mSSDOBiasFalloffNear" => "SSDO bias 近处衰减。",
            "mSSDOIntensityMin" => "SSDO 强度最小值。",
            "mSSDOIntensityMax" => "SSDO 强度最大值。",
            "mSSDOBlurFalloff" => "SSDO 模糊衰减距离。",
            "mSSDOBlurSharpnessMin" => "SSDO 模糊锐度最小值。",
            "mSSDOBlurSharpnessMax" => "SSDO 模糊锐度最大值。",
            "mFXAAQualitySubpix" => "FXAA 子像素抗锯齿强度。",
            "mFXAAQualityEdgeThreshold" => "FXAA 边缘检测阈值。",
            "mFXAAQualityEdgeThresholdMin" => "FXAA 最小边缘阈值。",
            "TEXTURE_FILTER" => "material_common 贴图采样过滤；ANISOTROPIC 最清晰但成本略高。",
            "TEXTURE_MIP_FILTER" => "material_common mipmap 过滤；影响远处贴图稳定性。",
            "TEXTURE_ANISOTROPY_LEVEL" => "各向异性过滤等级；高角度表面更清晰，16 是 Ray 默认。",
            "ALPHA_THRESHOLD" => "Alpha 裁剪阈值；越接近 1 越严格，改动会影响透明/镂空边缘。",
            _ => "导出时只修改副本中的同名参数，未列出的 Ray 原始内容保持不变。",
        };
    }

    private int MeasureWrappedTextHeight(string text, int width)
    {
        var measured = TextRenderer.MeasureText(
            text,
            Font,
            new Size(Math.Max(120, width), int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
        return Math.Clamp(measured.Height + 4, 34, 86);
    }

    private void RefreshPreview()
    {
        RefreshDocumentPaths();
        var result = _compiler.Compile(_canvas.Graph);
        var advancedResult = IsAdvancedMode() ? _advancedCompiler.Compile(_document) : null;
        var visibleCompileMessages = GetVisibleCompileMessages(result.Messages);
        var compileUsable = result.Success || (IsAdvancedMode() && visibleCompileMessages.Count == 0);
        var compatibilityIssues = compileUsable
            ? RayCompatibilityChecker.Check(_document, result.MaterialText, advancedResult)
            : [];
        var materialText = result.MaterialText;
        if (visibleCompileMessages.Count > 0 || compatibilityIssues.Count > 0)
        {
            var messages = visibleCompileMessages
                .Select(message => $"// 编译: {message}")
                .Concat(compatibilityIssues.Select(message => $"// 兼容性: {message}"));
            materialText = string.Join(Environment.NewLine, messages) +
                           Environment.NewLine + Environment.NewLine + materialText;
        }

        UpdatePreviewFiles(result, materialText, advancedResult, compatibilityIssues);
        RefreshProblems(result, advancedResult, compatibilityIssues, visibleCompileMessages);
        SetStatus(result.Success && compatibilityIssues.Count == 0 ? "Ray 材质预览已生成。" : "Ray 材质存在需要处理的问题。");
        ScheduleAutoExport();
    }

    private void ScheduleAutoExport()
    {
        if (!_document.AutoExportEnabled || _isAutoExporting)
        {
            _autoExportTimer.Stop();
            return;
        }

        _autoExportTimer.Stop();
        _autoExportTimer.Start();
    }

    private void RunAutoExport()
    {
        if (!_document.AutoExportEnabled || _isAutoExporting)
        {
            return;
        }

        _isAutoExporting = true;
        try
        {
            if (TryExportPresetCore(interactive: false, copyFullPackage: false, out var message))
            {
                SetStatus($"自动导出完成：{message}");
            }
            else if (!string.IsNullOrWhiteSpace(message))
            {
                SetStatus($"自动导出跳过：{message}");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            SetStatus($"自动导出失败：{ex.Message}");
        }
        finally
        {
            _isAutoExporting = false;
        }
    }

    private void RefreshProblems(
        RayMaterialCompileResult result,
        RayAdvancedMaterialCompileResult? advancedResult,
        IReadOnlyList<string> compatibilityIssues,
        IReadOnlyList<string> visibleCompileMessages)
    {
        _problemItems.Clear();
        if (!_document.Graph.Nodes.Any(node => node.Kind == NodeKind.RayMaterialOutput))
        {
            _problemItems.Add(new RayProblemItem("错误", "缺少 Ray 材质输出节点。", null));
        }

        foreach (var message in visibleCompileMessages)
        {
            _problemItems.Add(new RayProblemItem(result.Success ? "提示" : "错误", message, null));
        }

        foreach (var message in compatibilityIssues)
        {
            _problemItems.Add(new RayProblemItem("兼容性", message, FindIssueNode(message)));
        }

        foreach (var node in _document.Graph.Nodes.Where(node => node.Kind == NodeKind.RayTextureSlot))
        {
            AddTextureProblems(node);
        }

        if (advancedResult is { Success: false })
        {
            foreach (var message in advancedResult.Messages)
            {
                _problemItems.Add(new RayProblemItem("高级", message, null));
            }
        }

        if (_problemItems.Count == 0)
        {
            _problemItems.Add(new RayProblemItem("OK", "未发现明显问题。", null));
        }

        _problemListBox.BeginUpdate();
        _problemListBox.Items.Clear();
        foreach (var item in _problemItems)
        {
            _problemListBox.Items.Add(item);
        }
        _problemListBox.EndUpdate();
    }

    private IReadOnlyList<string> GetVisibleCompileMessages(IReadOnlyList<string> messages)
    {
        if (!IsAdvancedMode())
        {
            return messages;
        }

        return messages
            .Where(message => !message.EndsWith("is not part of the Ray-MMD compatible node set.", StringComparison.Ordinal))
            .ToList();
    }

    private void AddTextureProblems(GraphNode node)
    {
        var source = ReadNodeProperty(node, "Source", "File");
        var file = ReadNodeProperty(node, "File", string.Empty);
        if (!RayCompatibilityChecker.IsFileSource(source))
        {
            if (IsAdvancedMode() && _document.Graph.GetOutgoing(node.Id).Any())
            {
                _problemItems.Add(new RayProblemItem("高级", "非文件来源贴图参与高级表达式时暂不支持，请改成文件贴图或走兼容参数。", node.Id));
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(file))
        {
            _problemItems.Add(new RayProblemItem("贴图", "Ray 贴图槽使用文件贴图，但 File 为空。", node.Id));
            return;
        }

        var absolute = RayCompatibilityChecker.ResolveTexturePath(_document, file);
        if (absolute is null || !File.Exists(absolute))
        {
            _problemItems.Add(new RayProblemItem("贴图", $"找不到贴图文件：{file}", node.Id));
        }
    }

    private Guid? FindIssueNode(string message)
    {
        if (message.StartsWith("找不到贴图文件：", StringComparison.Ordinal))
        {
            var file = message["找不到贴图文件：".Length..];
            return _document.Graph.Nodes
                .FirstOrDefault(node => node.Kind == NodeKind.RayTextureSlot &&
                                        string.Equals(ReadNodeProperty(node, "File", string.Empty), file, StringComparison.OrdinalIgnoreCase))
                ?.Id;
        }

        return _document.Graph.Nodes
            .FirstOrDefault(node => message.Contains(NodeRegistry.Get(node.Kind).Title, StringComparison.Ordinal))
            ?.Id;
    }

    private void SelectProblemNode()
    {
        if (_problemListBox.SelectedItem is not RayProblemItem { NodeId: { } nodeId })
        {
            return;
        }

        _canvas.SelectNodeById(nodeId);
        _rightTabs.SelectedIndex = 0;
    }

    private void UpdatePreviewFiles(
        RayMaterialCompileResult result,
        string materialText,
        RayAdvancedMaterialCompileResult? advancedResult,
        IReadOnlyList<string> compatibilityIssues)
    {
        var previous = _previewFileCombo.SelectedItem as string;
        _previewFiles.Clear();
        _previewFiles[RayExportNaming.GetMaterialFileName(_document)] = materialText;
        _previewFiles["material_common_2.0.fxsub"] = BuildMaterialCommonPreview(advancedResult);
        _previewFiles["ShadingMaterials.fxsub"] = RayAdvancedShadingPatcher.Preview(_document, advancedResult);
        _previewFiles["directional_lighting.fxsub"] = RayAdvancedLightingPatcher.Preview(_document);
        _previewFiles["ray.conf"] = BuildRayConfPreview();
        _previewFiles["ray_advanced.conf"] = BuildRayAdvancedConfPreview();
        _previewFiles["导出报告"] = RayExportReportWriter.Build(
            _document,
            result,
            new RayTextureExportResult(materialText, _previewFiles["material_common_2.0.fxsub"], [], []),
            compatibilityIssues,
            advancedResult,
            new RayLightingPatchResult(RayAdvancedLightingPatcher.IsEnabled(_document), [], []),
            new RayShadingPatchResult(advancedResult is not null && !string.IsNullOrWhiteSpace(advancedResult.ShadingPatchBlock), [], []),
            RayFeatureAnalyzer.Analyze(_document, applyAutoEnable: false));

        _previewFileCombo.BeginUpdate();
        _previewFileCombo.Items.Clear();
        foreach (var key in _previewFiles.Keys)
        {
            _previewFileCombo.Items.Add(key);
        }
        _previewFileCombo.EndUpdate();

        var selected = !string.IsNullOrWhiteSpace(previous) && _previewFiles.ContainsKey(previous)
            ? previous
            : RayExportNaming.GetMaterialFileName(_document);
        _previewFileCombo.SelectedItem = selected;
        _previewTextBox.Text = _previewFiles[selected];
    }

    private string BuildMaterialCommonPreview(RayAdvancedMaterialCompileResult? advancedResult)
    {
        var sourceCommon = Path.Combine(_document.RayRootPath, "Materials", "material_common_2.0.fxsub");
        if (!File.Exists(sourceCommon))
        {
            var patchPreview = advancedResult is null
                ? "// 当前不是高级节点模式。"
                : string.IsNullOrWhiteSpace(advancedResult.CommonPatchBlock)
                    ? "// 没有高级槽连接，导出时不会 patch common。"
                    : advancedResult.CommonPatchBlock;
            return $"// 找不到 Ray 原始文件：{sourceCommon}{Environment.NewLine}// 下方仅显示高级 patch 预览。{Environment.NewLine}{Environment.NewLine}{patchPreview}";
        }

        var text = RayConfigWriter.ApplyMaterialCommonDefines(File.ReadAllText(sourceCommon), _document.MaterialCommonValues);
        return advancedResult is null ? text : RayAdvancedCommonPatcher.Patch(text, advancedResult);
    }

    private string BuildRayConfPreview()
    {
        var sourceRayConf = Path.Combine(_document.RayRootPath, "ray.conf");
        if (!File.Exists(sourceRayConf))
        {
            return BuildMissingConfigPreview(sourceRayConf, _document.RayConfValues);
        }

        return RayConfigWriter.ApplyDefines(File.ReadAllText(sourceRayConf), _document.RayConfValues);
    }

    private string BuildRayAdvancedConfPreview()
    {
        var sourceAdvancedConf = Path.Combine(_document.RayRootPath, "ray_advanced.conf");
        if (!File.Exists(sourceAdvancedConf))
        {
            return BuildMissingConfigPreview(sourceAdvancedConf, _document.AdvancedConfValues);
        }

        return RayConfigWriter.ApplyStaticConstFloats(File.ReadAllText(sourceAdvancedConf), _document.AdvancedConfValues);
    }

    private static string BuildMissingConfigPreview(string path, IReadOnlyDictionary<string, string> values)
    {
        return $"// 找不到 Ray 原始文件：{path}{Environment.NewLine}// 当前面板将导出的参数值如下：{Environment.NewLine}" +
               string.Join(Environment.NewLine, values.Select(pair => $"// {pair.Key} = {pair.Value}"));
    }

    private void RefreshDocumentPaths()
    {
        _document.RayRootPath = _rayRootBox.Text;
        _document.ExportDirectory = _exportDirectoryBox.Text;
        _document.Graph = _canvas.Graph;
    }

    private void SetStatus(string text)
    {
        _statusLabel.Text = text;
    }

    private void ShowHelpDialog()
    {
        MessageBox.Show(
            this,
            "兼容模式：默认模式，只生成 Ray 原版 material_2.0.fx 可识别的参数，不 patch common。常量数学会在导出时折算，文件贴图会映射到 Ray MAP_* 参数。" +
            Environment.NewLine + Environment.NewLine +
            "高级节点模式：只修改导出副本里的 Materials/material_common_2.0.fxsub。文件贴图和部分数学/颜色/UV 节点可以生成逐像素 HLSL 表达式。" +
            Environment.NewLine + Environment.NewLine +
            "问题页：列出缺失贴图、模式不兼容、导出风险等项目。能定位到节点的问题可双击跳转。" +
            Environment.NewLine + Environment.NewLine +
            "预览页：可切换 material_2.0.fx、patched material_common、ray.conf、ray_advanced.conf 和导出报告。",
            "Ray 模式说明",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private static string ReadNodeProperty(GraphNode node, string name, string fallback)
    {
        return node.Properties.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private bool IsAdvancedMode()
    {
        return string.Equals(_document.MaterialMode, RayMaterialModes.Advanced, StringComparison.OrdinalIgnoreCase);
    }
}
