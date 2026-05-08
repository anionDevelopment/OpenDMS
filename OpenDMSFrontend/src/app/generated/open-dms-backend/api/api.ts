export * from './maintenanceRoutes.service';
import { MaintenanceRoutesService } from './maintenanceRoutes.service';
export * from './oIDC.service';
import { OIDCService } from './oIDC.service';
export * from './openDMSBackend.service';
import { OpenDMSBackendService } from './openDMSBackend.service';
export * from './user.service';
import { UserService } from './user.service';
export const APIS = [MaintenanceRoutesService, OIDCService, OpenDMSBackendService, UserService];
