import { string } from "yup/lib/locale"

export interface User{
    username: string,
    dispayName: string,
    token: string,
    image?: string
}

export interface UserFormValues{
    email: string,
    password: string,
    dispayName?: string,
    username?: string
}