export interface User {
    email: string
    password: string
    returnSecureToken?: boolean
}

export interface FireBaseResponse {
    idToken?: string
    expiresIn?: string
}

export interface Post {
    id?: string
    title: string,
    category: string
    text: string
    author: string
    date: Date
}