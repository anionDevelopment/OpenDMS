from ScriptCollection.TFCPS.Docker.TFCPS_CodeUnitSpecific_Docker import TFCPS_CodeUnitSpecific_Docker_Functions,TFCPS_CodeUnitSpecific_Docker_CLI


def on_building_finished():
    tf:TFCPS_CodeUnitSpecific_Docker_Functions=TFCPS_CodeUnitSpecific_Docker_CLI.parse(__file__)
    tf.verify_image_is_working(None,{"InitialDatabaseType":"Transient"},443,"/API/Other/Maintenance/AvailabilityCheck",True)
    tf._protected_sc.ensure_local_docker_network_does_not_exist("opendms_net")
    
if __name__ == "__main__":
    on_building_finished()
 