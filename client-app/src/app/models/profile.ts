import { User } from "./user";

export interface Profile{
    username: string,
    dispayName: string,
    image?: string,
    bio?: string
}

export class Profile implements Profile{
    constructor(user: User){
        this.username = user.username;;
        this.dispayName = user.dispayName;
        this.image = user.image;
    }
}