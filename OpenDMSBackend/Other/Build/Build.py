import sys
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure

def copy_ocr_resource_files(codeunit_folder:str,platforms:list[str]):
    src_folder=GeneralUtilities.resolve_relative_path("./Other/Resources/OCRData", codeunit_folder)
    for platform in platforms:
        dst_folder=GeneralUtilities.resolve_relative_path(f"./Other/Artifacts/BuildResult_DotNet_{platform}/OCRData/tessdata", codeunit_folder)
        GeneralUtilities.ensure_directory_exists(dst_folder)
        GeneralUtilities.copy_content_of_folder(src_folder, dst_folder,False,"^.*\\.traineddata$")

def build():
    t = TasksForCommonProjectStructure()
    file = str(Path(__file__).absolute())
    codeunit_folder=GeneralUtilities.resolve_relative_path("../../..", file)
    cmd_args = sys.argv
    verbosity = t.get_verbosity_from_commandline_arguments(cmd_args, 1)
    platforms=["win-x64", "linux-x64"]
    t.standardized_tasks_build_for_dotnet_project(file, "QualityCheck", t.get_default_target_environmenttype_mapping(), platforms, verbosity, cmd_args)
    t.generate_openapi_file(file, "win-x64", verbosity, cmd_args)
    copy_ocr_resource_files(codeunit_folder,platforms)


if __name__ == "__main__":
    build()
