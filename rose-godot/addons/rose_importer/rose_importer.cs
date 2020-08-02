using Godot;
using System;

[Tool]
public class rose_importer : EditorPlugin
{
    private EditorImportPlugin ZmdImporter = new Importers.ZmdImporter();
    private EditorImportPlugin ZmoImporter = new Importers.ZmoImporter();
    private EditorImportPlugin ZmsImporter = new Importers.ZmsImporter();

    public override void _EnterTree()
    {
        AddImportPlugin(ZmdImporter);
        AddImportPlugin(ZmoImporter);
        AddImportPlugin(ZmsImporter);
    }

    public override void _ExitTree()
    {
        RemoveImportPlugin(ZmdImporter);
        RemoveImportPlugin(ZmoImporter);
        RemoveImportPlugin(ZmsImporter);
        ZmdImporter = null;
        ZmoImporter = null;
        ZmsImporter = null;
    }
}
