from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def update_dependencies():
    current_file = str(Path(__file__).absolute())
    repository_folder = GeneralUtilities.resolve_relative_path("../../..", current_file)
    t: TasksForCommonProjectStructure = TasksForCommonProjectStructure()
    t.update_images_in_example(repository_folder)


if __name__ == "__main__":
    update_dependencies()
