export class ApiErrorResponseException extends Error {
  constructor(message: string = "") {
    super(message);
    this.name = "ApiErrorResponseException";
  }
}
