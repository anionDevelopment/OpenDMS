export * from './maintenanceRoutes.service';
import { MaintenanceRoutesService } from './maintenanceRoutes.service';
export * from './openDMSBackend.service';
import { OpenDMSBackendService } from './openDMSBackend.service';
export const APIS = [MaintenanceRoutesService, OpenDMSBackendService];
