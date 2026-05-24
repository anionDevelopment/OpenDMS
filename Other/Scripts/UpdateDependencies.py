from ScriptCollection.TFCPS.TFCPS_Generic import  TFCPS_Generic_CLI,TFCPS_Generic_Functions

def update_dependencies():
    t :TFCPS_Generic_Functions= TFCPS_Generic_CLI().parse(__file__)
    t.tfcps_Tools_General.update_dependent_oci_images(t.repository_folder)

if __name__ == "__main__":
    update_dependencies()
