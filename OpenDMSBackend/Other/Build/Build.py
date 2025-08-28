import sys
import os
from pathlib import Path
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure
from ScriptCollection.GeneralUtilities import GeneralUtilities


def build():
    t = TasksForCommonProjectStructure()
    file = str(Path(__file__).absolute())
    codeunit_folder = GeneralUtilities.resolve_relative_path("../../..", file)
    cmd_args = sys.argv
    verbosity = t.get_verbosity_from_commandline_arguments(cmd_args, 1)
    platforms = ["win-x64", "linux-x64"]
    t.standardized_tasks_build_for_dotnet_project(file, "QualityCheck", t.get_default_target_environmenttype_mapping(), platforms, verbosity, cmd_args)
    t.generate_openapi_file(file, "win-x64", verbosity, cmd_args)

    for platform in platforms:
        pass
        # TODO copy codeunit-resource "TypeScript" to output-directory
    # TODO add typescript to sbom


if __name__ == "__main__":
    build()
