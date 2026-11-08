
/* start of raceday profile picture cropping */

document.addEventListener("DOMContentLoaded", function () {

    //start of crop editor configuration

    const picker = document.getElementById("picturePicker");
    const canvas = document.getElementById("cropCanvas");
    const editor = document.getElementById("cropEditor");
    const currentPicture = document.getElementById("currentPicture");
    const zoomSlider = document.getElementById("imageZoom");
    const uploadForm = document.getElementById("profilePictureForm");
    const croppedInput = document.getElementById("croppedPicture");
    const saveButton = document.getElementById("savePictureButton");
    const status = document.getElementById("pictureStatus");

    if (!picker || !canvas || !uploadForm) {
        return;
    }

    const context = canvas.getContext("2d");

    const canvasSize = 512;

    let selectedImage = null;
    let zoom = 1;
    let offsetX = 0;
    let offsetY = 0;

    let dragging = false;
    let lastX = 0;
    let lastY = 0;

    let readyToSubmit = false;

    //end of crop editor configuration


    //keep the image covering the cropping area
    function limitPosition() {

        //start of method

        if (!selectedImage) {
            return;
        }

        const baseScale = Math.max(
            canvasSize / selectedImage.naturalWidth,
            canvasSize / selectedImage.naturalHeight
        );

        const scale = baseScale * zoom;

        const imageWidth = selectedImage.naturalWidth * scale;
        const imageHeight = selectedImage.naturalHeight * scale;

        const maxX = (imageWidth - canvasSize) / 2;
        const maxY = (imageHeight - canvasSize) / 2;

        offsetX = Math.max(-maxX, Math.min(maxX, offsetX));
        offsetY = Math.max(-maxY, Math.min(maxY, offsetY));

        //end of method
    }


    //draw the selected portion of the image
    function drawCrop() {

        //start of method

        if (!selectedImage) {
            return;
        }

        limitPosition();

        const baseScale = Math.max(
            canvasSize / selectedImage.naturalWidth,
            canvasSize / selectedImage.naturalHeight
        );

        const scale = baseScale * zoom;

        const imageWidth = selectedImage.naturalWidth * scale;
        const imageHeight = selectedImage.naturalHeight * scale;

        const x = (canvasSize - imageWidth) / 2 + offsetX;
        const y = (canvasSize - imageHeight) / 2 + offsetY;

        //white background for transparent PNG pictures
        context.fillStyle = "#FFFFFF";
        context.fillRect(0, 0, canvasSize, canvasSize);

        context.drawImage(
            selectedImage,
            x,
            y,
            imageWidth,
            imageHeight
        );

        //end of method
    }


    //start of image selection
    picker.addEventListener("change", function () {

        //start of event

        const file = picker.files[0];

        if (!file) {
            return;
        }

        //check the selected file type
        if (!["image/jpeg", "image/png", "image/webp"]
            .includes(file.type)) {

            status.textContent = "Choose a JPG, PNG or WebP picture.";
            saveButton.disabled = true;
            return;
        }

        //limit the original image to 10 MB before cropping
        if (file.size > 10 * 1024 * 1024) {

            status.textContent = "Choose a picture smaller than 10 MB.";
            saveButton.disabled = true;
            return;
        }

        saveButton.disabled = true;
        selectedImage = null;
        status.textContent = "Preparing your picture...";

        const imageUrl = URL.createObjectURL(file);
        const image = new Image();

        image.onload = function () {

            //start of event

            URL.revokeObjectURL(imageUrl);

            selectedImage = image;

            zoom = 1;
            offsetX = 0;
            offsetY = 0;

            zoomSlider.value = "1";

            //replace the old picture with the crop preview
            currentPicture.classList.add("d-none");
            editor.classList.remove("d-none");

            drawCrop();

            saveButton.disabled = false;

            status.textContent =
                "Drag and zoom until the picture looks right.";

            //end of event
        };

        image.onerror = function () {

            //start of event

            URL.revokeObjectURL(imageUrl);

            status.textContent =
                "This image could not be opened.";

            saveButton.disabled = true;

            //end of event
        };

        image.src = imageUrl;

        //end of event
    });
    //end of image selection


    //start of zoom control
    zoomSlider.addEventListener("input", function () {

        //start of event

        zoom = Number(zoomSlider.value);

        drawCrop();

        //end of event
    });
    //end of zoom control


    //start of drag controls
    canvas.addEventListener("pointerdown", function (event) {

        //start of event

        if (!selectedImage) {
            return;
        }

        dragging = true;

        lastX = event.clientX;
        lastY = event.clientY;

        canvas.setPointerCapture(event.pointerId);

        //end of event
    });

    canvas.addEventListener("pointermove", function (event) {

        //start of event

        if (!dragging || !selectedImage) {
            return;
        }

        const rectangle = canvas.getBoundingClientRect();

        const movementScale = canvasSize / rectangle.width;

        offsetX += (event.clientX - lastX) * movementScale;
        offsetY += (event.clientY - lastY) * movementScale;

        lastX = event.clientX;
        lastY = event.clientY;

        drawCrop();

        //end of event
    });

    canvas.addEventListener("pointerup", function () {

        dragging = false;

    });

    canvas.addEventListener("pointercancel", function () {

        dragging = false;

    });
    //end of drag controls


    //start of cropped image submission
    uploadForm.addEventListener("submit", async function (event) {

        //start of event

        //allow the second submission after cropping
        if (readyToSubmit) {
            return;
        }

        event.preventDefault();

        if (!selectedImage) {

            status.textContent = "Please select a picture.";
            return;
        }

        saveButton.disabled = true;
        status.textContent = "Saving your selected picture...";

        //convert the selected crop into a JPEG
        const croppedBlob = await new Promise(resolve => {

            canvas.toBlob(
                resolve,
                "image/jpeg",
                0.88
            );

        });

        if (!croppedBlob) {

            status.textContent = "Unable to crop this picture.";
            saveButton.disabled = false;
            return;
        }

        //check the cropped image size
        if (croppedBlob.size > 2 * 1024 * 1024) {

            status.textContent = "The cropped picture is too large.";
            saveButton.disabled = false;
            return;
        }

        //create a file containing the cropped picture
        const croppedFile = new File(
            [croppedBlob],
            "raceday-profile.jpg",
            { type: "image/jpeg" }
        );

        //attach the cropped picture to the upload form
        const files = new DataTransfer();

        files.items.add(croppedFile);

        croppedInput.files = files.files;

        //submit to the existing MVC upload controller
        readyToSubmit = true;

        saveButton.disabled = false;

        uploadForm.requestSubmit();

        //end of event
    });
    //end of cropped image submission

});

/* end of raceday profile picture cropping */
