import os
from pathlib import Path
from ScriptCollection.ScriptCollectionCore import ScriptCollectionCore
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.TFCPS.TFCPS_Generic import TFCPS_Generic_Functions, TFCPS_Generic_CLI

@GeneralUtilities.check_arguments
def get_docker_networks(sc:ScriptCollectionCore)->list[str]:
    program_result=sc.run_program("docker","network list")
    result=[]
    lines=program_result[1].split("\n")[1:]
    for line in lines:
        splitted=[item for item in line.split(' ') if GeneralUtilities.string_has_content(item)]
        result.append(splitted[1])
    return result

@GeneralUtilities.check_arguments
def ensure_docker_network_is_available(t :TFCPS_Generic_Functions,network_name:str):
    if not (network_name  in get_docker_networks(t.sc)):
        t.sc.run_program("docker",f"network create {network_name}")

@GeneralUtilities.check_arguments
def pull_images_of_test_services(t :TFCPS_Generic_Functions):
    test_services=[f for f in GeneralUtilities.get_direct_folders_of_folder(os.path.join(t.repository_folder,"Other","Resources","LocalTestServices"))]
    if 0<len(test_services):
       t.sc.log.log("Pull images for local test-services...")
    for test_service_folder in test_services:
        test_service_name=os.path.basename(test_service_folder)
        t.sc.log.log(f"Pull image for test-service {test_service_name}...")
        t.sc.run_program("docker",f"compose -f docker-compose.yml pull --quiet",test_service_folder,print_live_output=True)

def prepare_build_codeunits():
    t :TFCPS_Generic_Functions= TFCPS_Generic_CLI().parse(__file__)
    t.tfcps_Tools_General.ensure_certificate_authority_for_development_purposes_is_generated(t.repository_folder)
    t.tfcps_Tools_General.generate_certificate_for_development_purposes_for_product(t.repository_folder)
    t.tfcps_Tools_General.generate_tasksfile_from_workspace_file(t.repository_folder)
    t.tfcps_Tools_General.generate_codeunits_overview_diagram(t.repository_folder)
    t.tfcps_Tools_General.generate_svg_files_from_plantuml_files_for_repository(t.repository_folder,t.use_cache())
    t.tfcps_Tools_General.do_npm_install(os.path.join(t.repository_folder, "Other", "Resources", "TypeScript"),True,t.use_cache())
    ensure_docker_network_is_available(t,"opendms_net")
    pull_images_of_test_services(t)


if __name__ == "__main__":
    prepare_build_codeunits()
