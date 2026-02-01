import sys
from pathlib import Path
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore
from ScriptCollection.TFCPS.TFCPS_Tools_General import TFCPS_Tools_General


def start_dockerfile_example():
    current_file = str(Path(__file__).absolute())
    env_file_name = "Variables.env"
    sc=ScriptCollectionCore()
    t: TFCPS_Tools_General = TFCPS_Tools_General(sc)
    t.ensure_env_file_is_generated(current_file, env_file_name, dict({
        'InitialAdminPassword': 'admin',
        'InitialDatabaseType': 'PostgreSQL',
        'InitialDatabaseConnectionString': 'Host=opendms_database;Port=5432;Database=OpenDMSDatabase;Username=root;Password=R00tpa55w0rd;',
        'InitialOCRDataServiceAddress': 'https://localhost:448',
        "image_postgres":sc.get_image_with_registry_for_docker_image("postgres",None,sc.default_fallback_docker_registry),
        "image_simpleocr":sc.get_image_with_registry_for_docker_image("simpleocr",None,sc.default_fallback_docker_registry),
        "image_adminer":sc.get_image_with_registry_for_docker_image("adminer",None,sc.default_fallback_docker_registry),
    }))
    t.start_dockerfile_example(current_file, True, True, env_file_name)


if __name__ == "__main__":
    start_dockerfile_example()
