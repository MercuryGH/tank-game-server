create database tank_game;

create table account(
    id varchar(20) primary key,
    pw varchar(40) not null,
);

-- 房间信息

create table player(
    id varchar(20) primary key,
    info json
);

-- 居然在MySQL里面用 JSON，MongoDB狂喜