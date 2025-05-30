export interface CellProps {
    changeColor:boolean,
    getPlayer:()=>Player
    getMove:(position:number,value:number)=>void
    position:number
    allowClick:boolean
}

export enum Player{
    O=0,X=1,D
}
export interface WonProps
{
   player:Player
    animate:boolean
}

export interface SmallBoardProps{
    macroOrder:number
    board:number[]
    updateBoard:(value:number[],macro:number,isWon:boolean)=>void
}