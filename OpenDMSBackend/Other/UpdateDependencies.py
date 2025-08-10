import sys
import os
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def update_dependencies():
    t = TasksForCommonProjectStructure()
    script_file: str = str(Path(__file__).absolute())
    t.update_dependencies_of_typical_dotnet_codeunit(script_file, 1, sys.argv)
    codeunit_folder = GeneralUtilities.resolve_relative_path("..", os.path.dirname(script_file))
    t.update_dependencies_of_typical_python_repository_requirements(codeunit_folder, 1, sys.argv)


if __name__ == "__main__":
    update_dependencies()
