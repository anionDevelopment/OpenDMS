import os
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def start_local_test_service():
    TasksForCommonProjectStructure().start_local_test_service(os.path.abspath(__file__))


if __name__ == "__main__":
    start_local_test_service()
