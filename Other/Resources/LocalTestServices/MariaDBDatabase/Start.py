import os
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore


def start_local_test_service():
    ScriptCollectionCore().start_local_test_service(os.path.abspath(__file__))


if __name__ == "__main__":
    start_local_test_service()
 