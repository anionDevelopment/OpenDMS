import sys
import os
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore
from ScriptCollection.TFCPS.TFCPS_Tools_General import TFCPS_Tools_General
from ScriptCollection.ImageUpdater import ImageUpdaterHelper, ConcreteImageUpdaterForDebian

def update_debian_version():
    concreteImageUpdaterForDebian=ConcreteImageUpdaterForDebian()
    latest_debian_tag=concreteImageUpdaterForDebian.version_to_tag(ImageUpdaterHelper.get_latest_version(concreteImageUpdaterForDebian.get_all_available_versions("debian")))

    current_file = str(Path(__file__).absolute())
    repository_folder = GeneralUtilities.resolve_relative_path("../../..", current_file)
    debian_version_file: str = GeneralUtilities.resolve_relative_path("Other/Resources/Dependencies/Debian/Version.txt", repository_folder)
    GeneralUtilities.write_text_to_file(debian_version_file, latest_debian_tag)


def update_dependencies():
    current_file = str(Path(__file__).absolute())
    repository_folder = GeneralUtilities.resolve_relative_path("../../..", current_file)
    sc=ScriptCollectionCore()
    t: TFCPS_Tools_General = TFCPS_Tools_General(sc)
    update_debian_version()
    typescript_resource_folder: str = os.path.join(repository_folder, "Other", "Resources", "TypeScript")
    t.update_dependencies_of_package_json(typescript_resource_folder, 1, sys.argv)

if __name__ == "__main__":
    update_dependencies()
