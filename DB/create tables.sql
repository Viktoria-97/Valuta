CREATE TABLE Valuta(
        ID VARCHAR PRIMARY KEY,
        Name VARCHAR NOT NULL,
        EngName VARCHAR NOT NULL,
        Nominal INTEGER NOT NULL,
        ParentCode VARCHAR NOT NULL,
        ISO_Num_Code INTEGER NOT NULL,
        ISO_Char_Code VARCHAR NOT NULL
);
CREATE TABLE ValCurs
 (
    ID_val VARCHAR NOT NULL,
    Value VARCHAR NOT NULL,
    Date_req TEXT NOT NULL,
    FOREIGN KEY (ID_val) REFERENCES Valuta(ID)
 );