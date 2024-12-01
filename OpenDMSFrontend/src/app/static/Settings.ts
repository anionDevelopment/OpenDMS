import { environment } from "../../environments/environment";
import packageInfo from '../../../package.json';
export class Settings {
    public static getAPIUrl(): string {
        if (environment.apiUrl) {
            return environment.apiUrl;
        } else {
            throw Error("environment.apiURL is not defined.");
        }
    }
    public static isVerbose(): boolean {
        if (environment.verbose) {
            return environment.verbose;
        } else {
            throw Error("environment.verbose is not defined.");
        }
    }
    public static isProduction(): boolean {
        if (environment.production) {
            return environment.production;
        } else {
            throw Error("environment.production is not defined.");
        }
    }
    public static getAppName(): string {
        return "OpenDMS";
    }
    public static getAppVersion(): string {
        return packageInfo.version;
    }
}
