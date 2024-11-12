from pathlib import Path
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def prepare_build_codeunits():
    t = TasksForCommonProjectStructure()
    current_file = str(Path(__file__).absolute())
    t.ensure_certificate_authority_for_development_purposes_is_generated(current_file)


if __name__ == "__main__":
    prepare_build_codeunits()
