export class ApiErrorResponseException extends Error {
  messages: string[];

  constructor(messages: string[] = [], message: string = "") {
    super(message);
    this.name = "ApiErrorResponseException";
    this.messages = messages;
  }
}
