import sys
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def generate_reference():
    file: str = str(Path(__file__).absolute())
    codeunit_folder: str = GeneralUtilities.resolve_relative_path("../../..", file)
    t = TasksForCommonProjectStructure()
    t.generate_svg_files_from_plantuml_files_for_codeunit(codeunit_folder)
    t.standardized_tasks_generate_reference_by_docfx(file, 1, "QualityCheck", sys.argv)


if __name__ == "__main__":
    generate_reference()
