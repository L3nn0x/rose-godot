using Godot;
using Importers;

[Tool]
public class rose_importer : EditorPlugin
{
    private EditorImportPlugin HimImporter = new HimImporter();
    private EditorImportPlugin ZmdImporter = new ZmdImporter();
    private EditorImportPlugin ZmoImporter = new ZmoImporter();
    private EditorImportPlugin ZmsImporter = new ZmsImporter();
    private EditorImportPlugin ZonImporter = new ZonImporter();

    public override void _EnterTree()
    {
        AddImportPlugin(HimImporter);
        AddImportPlugin(ZmdImporter);
        AddImportPlugin(ZmoImporter);
        AddImportPlugin(ZmsImporter);
        AddImportPlugin(ZonImporter);
    }

    public override void _ExitTree()
    {
        RemoveImportPlugin(ZmdImporter);
        RemoveImportPlugin(ZmoImporter);
        RemoveImportPlugin(ZmsImporter);
        RemoveImportPlugin(ZonImporter);
        RemoveImportPlugin(HimImporter);
        ZmdImporter = null;
        ZmoImporter = null;
        ZmsImporter = null;
        ZonImporter = null;
        HimImporter = null;
    }
}
