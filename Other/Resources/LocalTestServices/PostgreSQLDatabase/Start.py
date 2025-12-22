import os
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore


def start_local_test_service():
    current_file=os.path.abspath(__file__)
    current_folder=os.path.dirname(current_file)
    volumes_folder=os.path.join(current_folder,"Volumes")
    GeneralUtilities.ensure_directory_does_not_exist(volumes_folder)
    sc=ScriptCollectionCore()
    sc.start_local_test_service(current_file)


if __name__ == "__main__":
    start_local_test_service()
