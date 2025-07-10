import sys
from pathlib import Path
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def update_dependencies():
    t: TasksForCommonProjectStructure = TasksForCommonProjectStructure()
    update_dependencies_script_file = str(Path(__file__).absolute())
    t.update_dependencies_of_typical_node_codeunit(update_dependencies_script_file, 1, sys.argv)
    t.set_version_of_openapigenerator_by_update_dependencies_file(update_dependencies_script_file)


if __name__ == "__main__":
    update_dependencies()
