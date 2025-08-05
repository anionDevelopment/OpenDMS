import sys
from pathlib import Path
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def build():
    t = TasksForCommonProjectStructure()
    file = str(Path(__file__).absolute())
    cmd_args = sys.argv
    verbosity = t.get_verbosity_from_commandline_arguments(cmd_args, 1)
    platforms = ["win-x64", "linux-x64"]
    t.standardized_tasks_build_for_dotnet_project(file, "QualityCheck", t.get_default_target_environmenttype_mapping(), platforms, verbosity, cmd_args)
    t.generate_openapi_file(file, "win-x64", verbosity, cmd_args)
    for platform in platforms:
        pass  # TODO copy codeunit-resource "TypeScript" to output-directory


if __name__ == "__main__":
    build()
