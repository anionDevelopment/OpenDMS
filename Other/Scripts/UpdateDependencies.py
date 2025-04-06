from pathlib import Path
import re
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure

def update_dependencies():
    current_file = str(Path(__file__).absolute())
    repository_folder = GeneralUtilities.resolve_relative_path("../../..", current_file)
    t:TasksForCommonProjectStructure=TasksForCommonProjectStructure()
    t.update_iplocation_submodule(repository_folder,"tessdata:best")


if __name__ == "__main__":
    update_dependencies()
