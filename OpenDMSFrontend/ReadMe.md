# OpenDMSFrontend

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) and currently uses Angular 22.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Running unit tests

Run `ng test` to execute the unit tests via [Vitest](https://vitest.dev).

## Running visual-regression-tests

The visual-regression-tests are located in [e2e](./e2e) and are written with [Playwright](https://playwright.dev). They are
executed by `Other/QualityCheck/RunTestcases.py` together with the unit-tests, always inside a container, because a screenshot
depends on the operating-system it was rendered on. If the appearance of a page was changed intentionally then the
baseline-screenshots have to be regenerated with the task `UpdateVisualRegressionBaselines` of the repository.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
