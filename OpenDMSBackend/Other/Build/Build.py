import sys
from pathlib import Path
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def build():
    t = TasksForCommonProjectStructure()
    file = str(Path(__file__).absolute())
    cmd_args = sys.argv
    runtime = "win-x64"
    verbosity = t.get_verbosity_from_commandline_arguments(cmd_args, 1)
    t.standardized_tasks_build_for_dotnet_project(
        str(Path(__file__).absolute()), "QualityCheck", t.get_default_target_environmenttype_mapping(), [runtime], verbosity, cmd_args)
    t.generate_openapi_file(file, runtime, verbosity, cmd_args)


if __name__ == "__main__":
    build()
