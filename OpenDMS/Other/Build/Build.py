import sys
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def build():
    t = TasksForCommonProjectStructure()
    build_script_file = str(Path(__file__).absolute())
    codeunit_folder: str = GeneralUtilities.resolve_relative_path("../../..", build_script_file)

    custom_args: dict[str, str] = dict[str, str]()
    t.standardized_tasks_build_for_docker_project(build_script_file, "QualityCheck", 1, sys.argv, custom_args)
    t.merge_sbom_file_from_dependent_codeunit_into_this(build_script_file, "OpenDMSBackend")
    t.merge_sbom_file_from_dependent_codeunit_into_this(build_script_file, "OpenDMSFrontend")


if __name__ == "__main__":
    build()
