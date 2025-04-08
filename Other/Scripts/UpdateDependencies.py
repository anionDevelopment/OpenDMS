from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure

def update_dependencies():
    current_file = str(Path(__file__).absolute())
    repository_folder = GeneralUtilities.resolve_relative_path("../../..", current_file)
    t:TasksForCommonProjectStructure=TasksForCommonProjectStructure()
    t.update_submodule(repository_folder,"tessdata_best")


if __name__ == "__main__":
    update_dependencies()
 