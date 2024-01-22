export class ApiError extends Error {
  constructor(
    public message: string,
    public errors: string[],
    public statusCode: number,
  ) {
    super(message);
    this.name = "ApiError";
  }
}
