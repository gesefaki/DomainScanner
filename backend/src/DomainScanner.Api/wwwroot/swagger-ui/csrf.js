window.csrfRequestIntecreptor = async function (request) {
    const unsafeMethods = ["POST", "PUT", "PATCH", "DELETE"];
    const method = (request.method ?? "GET").toUpperCase();
    
    request.credentials = "include";
    
    if (!unsafeMethods.includes(method)) {
        return request;
    }
    
    const response = await fetch("/api/v1/auth/csrf", {
        method: "GET",
        credentials: "include"
    });
    
    if (!response.ok) {
        throw new Error("Unable to obtain CSRF token.");
    }
    
    const { token } = await response.json();
    
    request.headers ??= {};
    request.headers["X-CSRF-TOKEN"] = token;
    
    return request;
}