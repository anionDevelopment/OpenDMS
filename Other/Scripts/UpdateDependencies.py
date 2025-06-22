from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def update_images_in_example(repository_folder: str):
    iu = ImageUpdater()
    iu.add_default_mapper()
    dockercomposefile: str = f"{repository_folder}\\Other\\Reference\\ReferenceContent\\Examples\\MinimalDockerComposeFile\\docker-compose.yml"
    excluded = ["opendms"]
    iu.update_all_services_in_docker_compose_file(dockercomposefile, VersionEcholon.LatestPatchOrLatestMinor, excluded)
    iu.check_for_newest_version(dockercomposefile, excluded)


def update_dependencies():
    current_file = str(Path(__file__).absolute())
    repository_folder = GeneralUtilities.resolve_relative_path("../../..", current_file)
    t: TasksForCommonProjectStructure = TasksForCommonProjectStructure()
    update_images_in_example(repository_folder)


if __name__ == "__main__":
    update_dependencies()
