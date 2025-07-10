import os
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def stop_local_test_service():
    TasksForCommonProjectStructure().stop_local_test_service(os.path.abspath(__file__))


if __name__ == "__main__":
    stop_local_test_service()
