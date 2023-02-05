export interface User {
    email: string
    password: string
    returnSecureToken?: boolean
}

export interface FireBaseResponse {
    idToken?: string
    expiresIn?: string
}