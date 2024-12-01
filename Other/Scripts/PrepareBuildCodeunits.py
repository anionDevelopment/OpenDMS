from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def prepare_build_codeunits():
    t = TasksForCommonProjectStructure()
    current_file = str(Path(__file__).absolute())
    product_folder: str = GeneralUtilities.resolve_relative_path("../../..", current_file)
    t.ensure_certificate_authority_for_development_purposes_is_generated(product_folder)


if __name__ == "__main__":
    prepare_build_codeunits()
