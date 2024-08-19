document.addEventListener("DOMContentLoaded", function() {
    const uploadForm = document.getElementById("uploadForm");
    const searchMusicButton = document.getElementById("searchMusicButton");
    const musicList = document.getElementById("musicList");
    const audioPlayer = document.getElementById("audioPlayer");
    let currentUrl = null;

    // Handle form submission for uploading music
    uploadForm.addEventListener("submit", async function(event) {
        event.preventDefault();

        // Получаем значения названия песни и автора
        const musicName = document.getElementById("songTitle").value.trim();
        const author = document.getElementById("artistName").value.trim();

        if (!musicName || !author) {
            alert("Please enter both song title and author.");
            return;
        }

        // Получаем выбранный файл
        const musicFile = document.getElementById("musicFile").files[0];
        if (!musicFile) {
            alert("Please choose a music file.");
            return;
        }

        // Создаем объект FormData для загрузки файла в body
        const formData = new FormData();
        formData.append("musicFile", musicFile);

        try {
            // Формируем URL с параметрами в query string
            const url = `https://localhost:7159/api/Order/UploadMusic?musicName=${encodeURIComponent(musicName)}&author=${encodeURIComponent(author)}`;

            // Отправляем POST-запрос
            const response = await fetch(url, {
                method: "POST",
                body: formData
            });

            if (response.ok) {
                alert("Music uploaded successfully!");
            } else {
                alert("Failed to upload music.");
            }
        } catch (error) {
            console.error("Error:", error);
            alert("An error occurred while uploading the music.");
        }
    });

    // Handle search and display potential music matches
    searchMusicButton.addEventListener("click", async function() {
        const request = document.getElementById("searchMusic").value.trim();
        if (!request) {
            alert("Please enter part of the song name.");
            return;
        }
    
        try {
            const response = await fetch(`https://localhost:7159/api/Order/GetListMusic?request=${encodeURIComponent(request)}`);
            if (response.ok) {
                const musicListData = await response.json();
                musicList.innerHTML = ""; // Clear previous list
    
                musicListData.forEach(music => {
                    // Предположим, что music - это объект, и у него есть свойства title и artist
                    const listItem = document.createElement("li");
                    listItem.textContent = `${music.musicName} - ${music.author}`; // Пример отображения
                    listItem.addEventListener("click", function() {
                        playMusic(music);
                    });
                    musicList.appendChild(listItem);
                });
            } else {
                alert("No matching songs found.");
            }
        } catch (error) {
            console.error("Error:", error);
            alert("An error occurred while searching for music.");
        }
    });
    

    // Function to play selected music
    async function playMusic(musicName) {
        try {
            // Убедитесь, что параметр musicName передается правильно
            const response = await fetch(`https://localhost:7159/api/Order/PlayMusic?musicName=${encodeURIComponent(musicName.musicName)}`);
            if (response.ok) {
                const blob = await response.blob();
                const newUrl = URL.createObjectURL(blob);

                // Release previous URL
                if (currentUrl) {
                    URL.revokeObjectURL(currentUrl);
                }

                // Set new URL and play music
                audioPlayer.src = newUrl;
                audioPlayer.play();

                currentUrl = newUrl;
            } else {
                alert("Failed to play music.");
            }
        } catch (error) {
            console.error("Error:", error);
            alert("An error occurred while loading the music.");
        }
    }
});
