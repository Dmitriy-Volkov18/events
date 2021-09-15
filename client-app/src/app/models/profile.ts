import { User } from "./user";

export interface Profile{
    username: string,
    dispayName: string,
    image?: string,
    bio?: string,
    followersCount: number,
    followingCount: number,
    following: boolean,
    photos?: Photo[]
}

export class Profile implements Profile{
    constructor(user: User){
        this.username = user.username;;
        this.dispayName = user.dispayName;
        this.image = user.image;
    }
}

export interface Photo{
    id: string,
    url: string,
    isMain: boolean
}