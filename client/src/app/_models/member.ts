import { Photo } from "./photo";

export interface Member {
    id: number;
    userName: string;
    age: number;
    knownAs: string;
    dateCreated: Date;
    lastActive: Date;
    gender: string;
    introduction: string;
    lookingFor: string;
    interest?: any;
    city: string;
    country: string;
    photos: Photo[];
    photoUrl: string;
}