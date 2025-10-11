import sys
import os
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def update_dependencies():
    current_file = str(Path(__file__).absolute())
    repository_folder = GeneralUtilities.resolve_relative_path("../../..", current_file)
    t: TasksForCommonProjectStructure = TasksForCommonProjectStructure()
    t.update_images_in_example(repository_folder)
    t.set_latest_version_for_clone_repository_as_resource("OCRData", "https://github.com/tesseract-ocr/tessdata")
    typescript_resource_folder: str = os.path.join(repository_folder, "Other", "Resources", "TypeScript")
    t.update_dependencies_of_package_json(typescript_resource_folder, 1, sys.argv)


if __name__ == "__main__":
    update_dependencies()
