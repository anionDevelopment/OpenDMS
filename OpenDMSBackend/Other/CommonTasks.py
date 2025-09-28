from ScriptCollection.TFCPS.DotNet.TFCPS_CodeUnitSpecific_DotNet import TFCPS_CodeUnitSpecific_DotNet_Functions,TFCPS_CodeUnitSpecific_DotNet_CLI
from ScriptCollection.TFCPS.DotNet.CertificateGeneratorInformationGenerate import CertificateGeneratorInformationGenerate

 
def common_tasks():
    tf:TFCPS_CodeUnitSpecific_DotNet_Functions=TFCPS_CodeUnitSpecific_DotNet_CLI.parse(__file__)    
    tf.tfcps_Tools_General.get_resource_from_global_resource(tf.get_codeunit_folder(), "DevelopmentCertificate")
    tf.tfcps_Tools_General.get_resource_from_global_resource(tf.get_codeunit_folder(), "OCRData")
    tf.do_common_tasks(tf.get_version_of_project(),CertificateGeneratorInformationGenerate())#codeunit-version should alsways be the same as project-version
""" 
def common_tasks_old():
    cmd_args = sys.argv
    t = TasksForCommonProjectStructure()
    sc = ScriptCollectionCore()
    build_environment = t.get_targetenvironmenttype_from_commandline_arguments(cmd_args, "QualityCheck")
    verbosity: int = t.get_verbosity_from_commandline_arguments(cmd_args, 1)
    file = str(Path(__file__).absolute())
    codeunit_folder = GeneralUtilities.resolve_relative_path("..", os.path.dirname(file))
    codeunit_name: str = os.path.basename(codeunit_folder)
    codeunit_version = sc.get_semver_version_from_gitversion(GeneralUtilities.resolve_relative_path("../..", os.path.dirname(file)))  # Should always be the same as the project-version
    folder_of_current_file = os.path.dirname(file)
    t.get_resource_from_global_resource(codeunit_folder, "OCRData")
    t.generate_certificate_for_development_purposes_for_codeunit(codeunit_folder)
    t.copy_resources_from_global_project_resources(codeunit_folder, "TypeScript")
    sc.replace_version_in_csproj_file(GeneralUtilities.resolve_relative_path(f"../{codeunit_name}/{codeunit_name}.csproj", folder_of_current_file), codeunit_version)
    sc.replace_version_in_csproj_file(GeneralUtilities.resolve_relative_path(f"../{codeunit_name}Tests/{codeunit_name}Tests.csproj", folder_of_current_file), codeunit_version)
    additional_arguments_file = t.get_additionalargumentsfile_from_commandline_arguments(cmd_args, None)
    #t.standardized_tasks_do_common_tasks(file, codeunit_version, verbosity, build_environment, True, additional_arguments_file, False, cmd_args)
    t.install_requirementstxt_for_codeunit(codeunit_folder)
    t.standardized_task_verify_standard_format_csproj_files(codeunit_folder)
    t.copy_development_certificate_to_default_development_directory(codeunit_folder, build_environment)
    t.set_constants_for_certificate_private_information(codeunit_folder)
    t.t4_transform(file, verbosity)
    t.update_year_for_dotnet_codeunit_in_common_scripts_file(file)
"""


if __name__ == "__main__":
    common_tasks()
